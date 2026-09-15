import { abrirMenu, entrarComo, expect, test } from '../suporte/fixtures';

test.describe('Permissões', () => {
  test('voluntário não vê o cadastro no menu e é barrado ao acessar direto', async ({ page }) => {
    await entrarComo(page, 'voluntario');

    await abrirMenu(page);
    await expect(page.getByRole('link', { name: 'Histórico' })).toBeVisible();
    await expect(page.getByRole('link', { name: 'Cadastro de Itens' })).toHaveCount(0);

    await page.goto('/cadastro');
    await expect(page).toHaveURL(/\/dashboard$/);
  });

  test('admin vê o cadastro no menu e consegue abrir', async ({ page }) => {
    await entrarComo(page, 'admin');

    await abrirMenu(page);
    await page.getByRole('link', { name: 'Cadastro de Itens' }).click();

    await expect(page.getByRole('heading', { name: 'Cadastro de itens' })).toBeVisible();
  });
});
