/**
 * Mechanics Software — Swagger UI Demo
 *
 * Executes the full service order lifecycle through Swagger UI and records the
 * session as a .webm video (saved to test-results/ after the run).
 *
 * Prerequisites:
 *   docker compose up -d        # API must be running at http://localhost:8080
 *
 * Run:
 *   cd scripts/swagger-demo
 *   npm install
 *   npx playwright install chromium
 *   npm run demo                 # headed (recommended for recording)
 *
 * Flow:
 *   1.  Login  → capture JWT
 *   2.  Authorize in Swagger UI
 *   3.  Create Customer
 *   4.  Create Vehicle
 *   5.  Create Service  (catalogue)
 *   6.  Create Part     (inventory)
 *   7.  Create Service Order
 *   8.  Start Diagnosis
 *   9.  Add Service Item
 *   10. Add Part Item
 *   11. Generate Budget
 *   12. Send Budget
 *   13. Approve
 *   14. Start Execution
 *   15. Complete
 *   16. Deliver
 *   17. Get Order  (final state)
 *   18. Average Execution Time metric
 */

import { test, Page } from '@playwright/test';

const BASE_URL   = 'http://localhost:8080';
const SWAGGER    = `${BASE_URL}/swagger/index.html`;
const ADMIN      = { email: 'admin@mechanics.local', password: 'Admin@123' };

// Shared token set after login — used by page.request calls inside runOp.
let JWT_TOKEN = '';

// ─── Random data generators ───────────────────────────────────────────────────

/** Generates a mathematically valid random CPF (11 digits). */
function generateCpf(): string {
  const d = Array.from({ length: 9 }, () => Math.floor(Math.random() * 10));

  const sum1 = d.reduce((acc, n, i) => acc + n * (10 - i), 0);
  const r1   = sum1 % 11;
  d.push(r1 < 2 ? 0 : 11 - r1);

  const sum2 = d.reduce((acc, n, i) => acc + n * (11 - i), 0);
  const r2   = sum2 % 11;
  d.push(r2 < 2 ? 0 : 11 - r2);

  // Reject all-same-digit CPFs (e.g. 00000000000), which are structurally valid
  // but rejected by the domain rule AllDigitsEqual().
  if (new Set(d).size === 1) return generateCpf();

  return d.join('');
}

/** Generates a valid Brazilian legacy-format license plate (ABC1234). */
function generatePlate(): string {
  const L = () => 'ABCDEFGHIJKLMNOPQRSTUVWXYZ'[Math.floor(Math.random() * 26)];
  const N = () => Math.floor(Math.random() * 10).toString();
  return `${L()}${L()}${L()}${N()}${N()}${N()}${N()}`;
}

/** Appends a ms timestamp so part codes are unique across runs. */
function generatePartCode(prefix: string): string {
  return `${prefix}-${Date.now()}`;
}

// ─── Timing helpers ──────────────────────────────────────────────────────────

const pause = (ms = 1400) => new Promise<void>(r => setTimeout(r, ms));

// ─── Swagger UI helpers ───────────────────────────────────────────────────────

/**
 * Build a RegExp that matches the exact path at the end of a text string.
 * Escapes `/`, `{`, `}` so the path can be used safely in a RegExp.
 */
function pathRegex(exactPath: string): RegExp {
  const escaped = exactPath.replace(/[/{}]/g, c => `\\${c}`);
  return new RegExp(`${escaped}$`);
}

/** Locate an operation block by HTTP method + exact path. */
function findOp(page: Page, method: string, exactPath: string) {
  return page
    .locator('.opblock')
    .filter({ has: page.locator('.opblock-summary-method', { hasText: method }) })
    .filter({ has: page.locator('.opblock-summary-path').filter({ hasText: pathRegex(exactPath) }) })
    .first();
}

/** Expand the operation block if it is currently collapsed. */
async function expand(page: Page, method: string, path: string) {
  const op = findOp(page, method, path);
  await op.scrollIntoViewIfNeeded();
  await pause(400);
  const isExpanded = await op.locator('.opblock-body').isVisible().catch(() => false);
  if (!isExpanded) {
    await op.locator('.opblock-summary').click();
    await pause(700);
  }
  return op;
}

/**
 * Open "Try it out", optionally fill path param and/or request body,
 * click Execute, wait for the response, scroll to it, and return the
 * raw JSON text from the response body.
 */
async function runOp(
  page: Page,
  method: string,
  path: string,
  opts: { body?: string; pathParam?: string } = {},
): Promise<string> {
  const resolvedPath = path.replace('{id}', opts.pathParam ?? '');
  const resolvedUrl  = `${BASE_URL}${resolvedPath}`;

  // ── Visual: show interaction in Swagger UI (for the video) ───────────────
  const op = await expand(page, method, path);
  await op.locator('.try-out__btn').click();
  await pause(700);

  if (opts.pathParam) {
    // Show the UUID in the input field visually.
    // We don't depend on React reading this value — page.request handles
    // the actual call below.
    const input = op.locator('.parameters-container input').first();
    await input.evaluate(
      (el: HTMLInputElement, val: string) => { el.value = val; },
      opts.pathParam,
    );
    await pause(500);
  }

  if (opts.body) {
    const textarea = op.locator('textarea.body-param__text');
    if (await textarea.count() > 0) {
      await textarea.click({ clickCount: 3 });
      await textarea.fill(opts.body);
      await pause(500);
    }
  }

  // Click Execute so it's visible in the video (may show validation error
  // for path params — that's OK, data comes from page.request below).
  await op.locator('button.execute.opblock-control__btn').click().catch(() => {});
  await pause(1000);

  // ── Data: direct API call — 100% reliable, no React state dependency ─────
  const response = await page.request.fetch(resolvedUrl, {
    method,
    headers: {
      'Authorization': `Bearer ${JWT_TOKEN}`,
      'Content-Type': 'application/json',
    },
    ...(opts.body ? { data: opts.body } : {}),
  });
  const body = await response.text();

  await pause(2000);
  await op.scrollIntoViewIfNeeded();
  await pause(1500);

  return body;
}

/** Parse the `id` field from a JSON response string. */
function extractId(raw: string): string {
  return JSON.parse(raw.trim()).id as string;
}

// ─── Test ─────────────────────────────────────────────────────────────────────

test('Fluxo completo — Ordem de Serviço', async ({ page }) => {

  // ── 0. Open Swagger UI ────────────────────────────────────────────────────
  await page.goto(SWAGGER);
  await page.waitForSelector('.swagger-ui .information-container', { timeout: 20_000 });
  await pause(2500);

  // ── 1. Login — intercept the API response directly to get the token ─────────
  //   (more reliable than parsing Swagger's syntax-highlighted response HTML)
  const loginResponsePromise = page.waitForResponse(
    r => r.url().includes('/api/auth/login') && r.status() === 200,
  );

  const loginBody = JSON.stringify(ADMIN, null, 2);
  await runOp(page, 'POST', '/api/auth/login', { body: loginBody });

  const token = ((await (await loginResponsePromise).json()) as { token: string }).token;
  JWT_TOKEN = token; // shared with page.request calls inside runOp

  // ── 2. Authorize in Swagger UI ────────────────────────────────────────────
  await page.locator('.auth-wrapper .authorize').click();
  await pause(1200);

  // Use the first input inside the auth form (.auth-container covers both
  // text and password input types across Swagger UI versions)
  await page.locator('.auth-container input').first().fill(token);
  await pause(600);

  // Click "Authorize" inside the modal (not the header button)
  await page.locator('.auth-container .authorize').click();
  await pause(900);

  // Close the modal
  await page.locator('.modal-ux .btn-done').click();
  await pause(1800);

  // ── 3. Create Customer ────────────────────────────────────────────────────
  const customerBody = JSON.stringify({
    name:           'Carlos Mecânica',
    documentValue:  generateCpf(),  // random valid CPF — unique per run
    personType:     0,              // INDIVIDUAL
    email:          'carlos@mecanica.com',
    phone:          '11987654321',
  }, null, 2);
  const customerId = extractId(await runOp(page, 'POST', '/api/customers', { body: customerBody }));

  // ── 4. Create Vehicle ─────────────────────────────────────────────────────
  const vehicleBody = JSON.stringify({
    licensePlate: generatePlate(),  // random valid plate — unique per run
    make:         'Toyota',
    model:        'Corolla',
    year:         2022,
    customerId,
  }, null, 2);
  const vehicleId = extractId(await runOp(page, 'POST', '/api/vehicles', { body: vehicleBody }));

  // ── 5. Create Service (catalogue) ─────────────────────────────────────────
  const serviceBody = JSON.stringify({
    name:               'Troca de Óleo',
    description:        'Troca de óleo do motor + filtro',
    basePriceInCents:   9000,   // R$ 90,00
    estimatedMinutes:   60,
  }, null, 2);
  const serviceId = extractId(await runOp(page, 'POST', '/api/services', { body: serviceBody }));

  // ── 6. Create Part (inventory) ────────────────────────────────────────────
  const partBody = JSON.stringify({
    code:             generatePartCode('OL-5W30'),  // unique per run
    name:             'Óleo Motor 5W30',
    description:      'Óleo de motor sintético 5W30 — 1L',
    unitPriceInCents: 4500,   // R$ 45,00
    initialStock:     50,
  }, null, 2);
  const partId = extractId(await runOp(page, 'POST', '/api/parts', { body: partBody }));

  // ── 7. Create Service Order ───────────────────────────────────────────────
  const soBody  = JSON.stringify({ customerId, vehicleId }, null, 2);
  const orderId = extractId(await runOp(page, 'POST', '/api/service-orders', { body: soBody }));

  // ── 8. Start Diagnosis ────────────────────────────────────────────────────
  await runOp(page, 'POST', '/api/service-orders/{id}/start-diagnosis', { pathParam: orderId });

  // ── 9. Add Service Item ───────────────────────────────────────────────────
  await runOp(page, 'POST', '/api/service-orders/{id}/services', {
    pathParam: orderId,
    body: JSON.stringify({ serviceId, quantity: 1 }, null, 2),
  });

  // ── 10. Add Part Item ─────────────────────────────────────────────────────
  await runOp(page, 'POST', '/api/service-orders/{id}/parts', {
    pathParam: orderId,
    body: JSON.stringify({ partId, quantity: 2 }, null, 2),
  });

  // ── 11. Generate Budget ───────────────────────────────────────────────────
  await runOp(page, 'POST', '/api/service-orders/{id}/budget', { pathParam: orderId });

  // ── 12. Send Budget ───────────────────────────────────────────────────────
  await runOp(page, 'POST', '/api/service-orders/{id}/send-budget', { pathParam: orderId });

  // ── 13. Approve ───────────────────────────────────────────────────────────
  await runOp(page, 'POST', '/api/service-orders/{id}/approve', { pathParam: orderId });

  // ── 14. Start Execution ───────────────────────────────────────────────────
  await runOp(page, 'POST', '/api/service-orders/{id}/start-execution', { pathParam: orderId });

  // ── 15. Complete ──────────────────────────────────────────────────────────
  await runOp(page, 'POST', '/api/service-orders/{id}/complete', { pathParam: orderId });

  // ── 16. Deliver ───────────────────────────────────────────────────────────
  await runOp(page, 'POST', '/api/service-orders/{id}/deliver', { pathParam: orderId });

  // ── 17. Get final order state ─────────────────────────────────────────────
  await runOp(page, 'GET', '/api/service-orders/{id}', { pathParam: orderId });

  // ── 18. Average execution time metric ────────────────────────────────────
  await runOp(page, 'GET', '/api/service-orders/metrics/average-execution-time');

  // Hold the final state visible on screen for the recording
  await pause(5000);
});
