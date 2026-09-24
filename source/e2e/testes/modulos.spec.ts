import { abrirMenu, entrarComo, expect, test } from '../suporte/fixtures';

test.describe('Módulos', () => {
  test('usuário dos Obreiros vê o módulo no cabeçalho e o login no menu', async ({ page }) => {
    await entrarComo(page, 'admin');

    await expect(page.getByText('· Obreiros')).toBeVisible();

    await abrirMenu(page);
    await expect(page.getByText('Conectado como admin')).toBeVisible();
    await expect(page.getByText(/^Versão /)).toBeVisible();
    await expect(page.getByRole('link', { name: 'Estoque da Ação Social' })).toHaveCount(0);
  });

  test('usuário da Ação Social entra na área dele e não acessa os Obreiros', async ({ page }) => {
    await entrarComo(page, 'social');

    await expect(page).toHaveURL(/\/acao-social$/);
    await expect(page.getByText('· Ação Social')).toBeVisible();

    await abrirMenu(page);
    await expect(page.getByRole('link', { name: 'Estoque da Ação Social' })).toBeVisible();
    await expect(page.getByRole('link', { name: 'Dashboard' })).toHaveCount(0);
    await expect(page.getByRole('link', { name: 'Histórico' })).toHaveCount(0);

    await page.goto('/dashboard');
    await expect(page).toHaveURL(/\/acao-social$/);
  });
});
