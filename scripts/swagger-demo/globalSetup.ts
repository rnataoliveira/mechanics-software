import { execSync } from 'child_process';
import http from 'http';
import path from 'path';

const PROJECT_ROOT = path.resolve(__dirname, '../..');
const API_HEALTH   = 'http://localhost:8080/swagger/index.html';

function probe(url: string): Promise<void> {
  return new Promise((resolve, reject) => {
    const req = http.get(url, res => {
      res.resume();
      res.statusCode && res.statusCode < 500 ? resolve() : reject(new Error(`HTTP ${res.statusCode}`));
    });
    req.setTimeout(3000, () => { req.destroy(); reject(new Error('timeout')); });
    req.on('error', reject);
  });
}

async function waitForApi(timeoutMs = 90_000): Promise<void> {
  const deadline = Date.now() + timeoutMs;
  let lastErr: unknown;
  while (Date.now() < deadline) {
    try { await probe(API_HEALTH); return; } catch (e) { lastErr = e; }
    await new Promise(r => setTimeout(r, 2000));
  }
  throw new Error(`API not ready after ${timeoutMs / 1000}s — ${lastErr}`);
}

export default async function globalSetup() {
  console.log('\n▶  Starting Docker containers…');
  execSync('docker compose up -d', { cwd: PROJECT_ROOT, stdio: 'inherit' });

  console.log('⏳  Waiting for API to be healthy…');
  await waitForApi();
  console.log('✅  API is ready!\n');
}
