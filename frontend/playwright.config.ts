import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: '.',
  testMatch: 'test-configuracoes.spec.ts',
  timeout: 90000,
  use: {
    baseURL: 'http://localhost:3000',
    navigationTimeout: 45000,
    actionTimeout: 15000,
    screenshot: 'only-on-failure',
  },
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],
});
