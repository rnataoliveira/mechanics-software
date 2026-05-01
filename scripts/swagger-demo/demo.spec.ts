/**
 * Mechanics Software — Swagger UI Demo
 *
 * Executes the full service order lifecycle through Swagger UI and records the
 * session as a .webm video (saved to test-results/ after the run).
 *
 * Prerequisites:
 *   docker compose up -d   # API + DB must be running at http://localhost:8080
 *                          # Seed data is created automatically on first boot.
 *
 * Run:
 *   cd scripts/swagger-demo
 *   npm install
 *   npx playwright install chromium
 *   npm run demo                 # headed (recommended for recording)
 *
 * Flow (14 steps):
 *   1.  Login  → capture JWT
 *   2.  Authorize in Swagger UI
 *   3.  Create Service Order      (pre-seeded customer + vehicle)
 *   4.  Start Diagnosis
 *   5.  Add Service Item          (pre-seeded service)
 *   6.  Add Part Item             (pre-seeded part)
 *   7.  Generate Budget
 *   8.  Send Budget
 *   9.  Approve
 *   10. Start Execution
 *   11. Complete
 *   12. Deliver
 *   13. Get Order  (final state)
 *   14. Average Execution Time metric
 *
 * Pre-seeded data (seeded automatically by DatabaseSeeder on first boot):
 *   Customer  Carlos Silva      — a1000000-0000-0000-0000-000000000001
 *   Vehicle   Toyota Corolla    — b1000000-0000-0000-0000-000000000001
 *   Service   Troca de Óleo     — c1000000-0000-0000-0000-000000000001
 *   Part      Óleo Motor 5W30   — d1000000-0000-0000-0000-000000000001
 */

import { test, Page } from '@playwright/test';

const BASE_URL = 'http://localhost:8080';
const SWAGGER  = `${BASE_URL}/swagger/index.html`;
const ADMIN    = { email: 'admin@mechanics.local', password: 'Admin@123' };

// Pre-seeded entity IDs — fixed by DatabaseSeeder, stable across restarts.
const SEED = {
  customerId: 'a1000000-0000-0000-0000-000000000001',
  vehicleId:  'b1000000-0000-0000-0000-000000000001',
  serviceId:  'c1000000-0000-0000-0000-000000000001',
  partId:     'd1000000-0000-0000-0000-000000000001',
} as const;

// Shared token set after login — used by page.request calls inside runOp.
let JWT_TOKEN = '';

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
  // Wait until the "Try it out" button is actually ready to receive clicks
  // (the block body animation must settle before the button is interactable).
  await op.locator('.try-out__btn').waitFor({ state: 'visible', timeout: 8000 });
  return op;
}

/**
 * Open "Try it out", fill path param and/or request body, then click Execute.
 *
 * pressSequentially types the UUID character-by-character so React's controlled
 * input state is updated — Execute works correctly for all endpoints.
 *
 * For endpoints WITHOUT a path param (login, create service order, metric):
 *   page.request is also called so the response body can be returned reliably
 *   (Swagger's response panel is harder to parse than a direct fetch).
 *
 * For endpoints WITH a path param (state-advancing POSTs, GET by id):
 *   Execute is the only call — calling the same endpoint twice via page.request
 *   would fail because the domain state has already been advanced.
 */
async function runOp(
  page: Page,
  method: string,
  path: string,
  opts: { body?: string; pathParam?: string } = {},
): Promise<string> {
  const resolvedPath = path.replace('{id}', opts.pathParam ?? '');
  const resolvedUrl  = `${BASE_URL}${resolvedPath}`;

  // ── Visual + Execute ──────────────────────────────────────────────────────
  const op = await expand(page, method, path);
  await op.locator('.try-out__btn').click();
  await pause(1000);

  if (opts.pathParam) {
    const input = op.locator('.parameters-container input').first();
    await input.waitFor({ state: 'visible', timeout: 5000 });
    await input.click({ clickCount: 3 });
    await input.pressSequentially(opts.pathParam, { delay: 20 });
    await pause(400);
  }

  if (opts.body) {
    const textarea = op.locator('textarea.body-param__text');
    await textarea.waitFor({ state: 'visible', timeout: 5000 });
    await textarea.click({ clickCount: 3 });
    await textarea.fill(opts.body);
    await pause(500);
  }

  await op.locator('button.execute.opblock-control__btn').click();

  // Wait for the response panel to render, then scroll it into view so the
  // recording captures the result. Fall back to scrolling the op block itself
  // if the live-responses selector doesn't appear in time.
  const responsePanel = op.locator('.live-responses-table');
  await responsePanel.waitFor({ state: 'visible', timeout: 8000 }).catch(() => null);
  await responsePanel.scrollIntoViewIfNeeded().catch(() => op.scrollIntoViewIfNeeded());
  await pause(2500);

  // ── Response body (only for non-path-param endpoints) ─────────────────────
  // For path-param endpoints Execute is the single call; calling page.request
  // again would hit an already-advanced domain state and return an error.
  if (opts.pathParam) return '';

  const response = await page.request.fetch(resolvedUrl, {
    method,
    headers: {
      'Authorization': `Bearer ${JWT_TOKEN}`,
      'Content-Type': 'application/json',
    },
    ...(opts.body ? { data: opts.body } : {}),
  });
  return response.text();
}

/** Parse the `id` field from a JSON response string; throws on missing id. */
function extractId(raw: string): string {
  let parsed: Record<string, unknown>;
  try {
    parsed = JSON.parse(raw.trim());
  } catch {
    throw new Error(`API response is not valid JSON: ${raw.slice(0, 200)}`);
  }
  const id = parsed.id as string | undefined;
  if (!id) throw new Error(`API response has no id: ${raw.slice(0, 200)}`);
  return id;
}

// ─── Test ─────────────────────────────────────────────────────────────────────

test('Fluxo completo — Ordem de Serviço', async ({ page }) => {

  // ── 0. Open Swagger UI ────────────────────────────────────────────────────
  await page.goto(SWAGGER);
  await page.waitForSelector('.swagger-ui .information-container', { timeout: 20_000 });
  await pause(2500);

  // ── 1. Login — intercept the API response directly to get the token ───────
  const loginResponsePromise = page.waitForResponse(
    r => r.url().includes('/api/auth/login') && r.status() === 200,
  );

  await runOp(page, 'POST', '/api/auth/login', {
    body: JSON.stringify(ADMIN, null, 2),
  });

  const token = ((await (await loginResponsePromise).json()) as { token: string }).token;
  JWT_TOKEN = token;

  // ── 2. Authorize in Swagger UI ────────────────────────────────────────────
  await page.locator('.auth-wrapper .authorize').click();
  await pause(1200);

  await page.locator('.auth-container input').first().fill(token);
  await pause(600);

  await page.locator('.auth-container .authorize').click();
  await pause(900);

  await page.locator('.modal-ux .btn-done').click();
  await pause(1800);

  // ── 3. Create Service Order ───────────────────────────────────────────────
  const orderId = extractId(
    await runOp(page, 'POST', '/api/service-orders', {
      body: JSON.stringify({ customerId: SEED.customerId, vehicleId: SEED.vehicleId }, null, 2),
    }),
  );

  // ── 4. Start Diagnosis ────────────────────────────────────────────────────
  await runOp(page, 'POST', '/api/service-orders/{id}/start-diagnosis', { pathParam: orderId });

  // ── 5. Add Service Item ───────────────────────────────────────────────────
  await runOp(page, 'POST', '/api/service-orders/{id}/services', {
    pathParam: orderId,
    body: JSON.stringify({ serviceId: SEED.serviceId, quantity: 1 }, null, 2),
  });

  // ── 6. Add Part Item ──────────────────────────────────────────────────────
  await runOp(page, 'POST', '/api/service-orders/{id}/parts', {
    pathParam: orderId,
    body: JSON.stringify({ partId: SEED.partId, quantity: 2 }, null, 2),
  });

  // ── 7. Generate Budget ────────────────────────────────────────────────────
  await runOp(page, 'POST', '/api/service-orders/{id}/budget', { pathParam: orderId });

  // ── 8. Send Budget ────────────────────────────────────────────────────────
  await runOp(page, 'POST', '/api/service-orders/{id}/send-budget', { pathParam: orderId });

  // ── 9. Approve ────────────────────────────────────────────────────────────
  await runOp(page, 'POST', '/api/service-orders/{id}/approve', { pathParam: orderId });

  // ── 10. Start Execution ───────────────────────────────────────────────────
  await runOp(page, 'POST', '/api/service-orders/{id}/start-execution', { pathParam: orderId });

  // ── 11. Complete ──────────────────────────────────────────────────────────
  await runOp(page, 'POST', '/api/service-orders/{id}/complete', { pathParam: orderId });

  // ── 12. Deliver ───────────────────────────────────────────────────────────
  await runOp(page, 'POST', '/api/service-orders/{id}/deliver', { pathParam: orderId });

  // ── 13. Get final order state ─────────────────────────────────────────────
  await runOp(page, 'GET', '/api/service-orders/{id}', { pathParam: orderId });

  // ── 14. Average execution time metric ─────────────────────────────────────
  await runOp(page, 'GET', '/api/service-orders/metrics/average-execution-time');

  await pause(5000);
});
