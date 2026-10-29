import { defineConfig, devices } from '@playwright/test';
import path from 'node:path';

/**
 * Tests de bout en bout (CAHIER-DES-CHARGES.md §15) sur une pile isolée : API sur le port 5288 avec sa propre base
 * (abs_clean_e2e, migrée au démarrage), site sur le port 4300. Rien ne part : envois en mode test.
 */
const API = 5288;
const WEB = 4300;

export default defineConfig({
  testDir: './tests',
  fullyParallel: true,
  forbidOnly: !!process.env['CI'],
  retries: process.env['CI'] ? 1 : 0,
  reporter: process.env['CI'] ? [['list'], ['html', { open: 'never' }]] : 'list',
  use: {
    baseURL: `http://localhost:${WEB}`,
    locale: 'fr-FR',
    trace: 'retain-on-failure',
  },
  projects: [{ name: 'chromium', use: { ...devices['Desktop Chrome'] } }],
  webServer: [
    {
      command: `dotnet run --project ../api --no-launch-profile --urls http://localhost:${API}`,
      url: `http://localhost:${API}/api/health`,
      timeout: 180_000,
      reuseExistingServer: !process.env['CI'],
      env: {
        ASPNETCORE_ENVIRONMENT: 'Development',
        ConnectionStrings__Default: process.env['E2E_CONNECTION_STRING'] ?? 'Host=localhost;Database=abs_clean_e2e',
        Database__MigrerAuDemarrage: 'true',
        RateLimit__Demandes__Limite: '10000',
        RateLimit__Global__LimiteParMinute: '100000',
        Site__Url: `http://localhost:${WEB}`,
        Envoi__Test__Dossier: path.resolve('test-results/boite-envoi'),
      },
    },
    {
      command: `npx ng serve --port ${WEB} --proxy-config proxy.e2e.json`,
      cwd: '../web',
      url: `http://localhost:${WEB}`,
      timeout: 180_000,
      reuseExistingServer: !process.env['CI'],
    },
  ],
});
