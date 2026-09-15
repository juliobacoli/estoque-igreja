import { defineConfig, devices } from '@playwright/test';

const noCi = !!process.env.CI;

export default defineConfig({
  testDir: './testes',

  // Todos os testes limpam o mesmo banco antes de rodar: um de cada vez.
  workers: 1,
  fullyParallel: false,

  // No CI, uma nova tentativa com trace; se só passar na segunda, o relatório marca como flaky.
  retries: noCi ? 1 : 0,
  forbidOnly: noCi,

  reporter: noCi ? [['list'], ['html', { open: 'never' }]] : [['list']],

  use: {
    baseURL: process.env.E2E_BASE_URL ?? 'http://localhost:5080',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
    video: 'retain-on-failure'
  },

  projects: [
    {
      // O app é usado no celular.
      name: 'celular',
      use: { ...devices['Pixel 7'] }
    }
  ]
});
