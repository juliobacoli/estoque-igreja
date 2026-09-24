import { abrirMenu, entrarComo, expect, test } from '../suporte/fixtures';

test.describe('Permissões', () => {
  test('admin vê o cadastro no menu e consegue abrir', async ({ page }) => {
    await entrarComo(page, 'admin');

    await abrirMenu(page);
    await page.getByRole('link', { name: 'Cadastro de Itens' }).click();

    await expect(page.getByRole('heading', { name: 'Cadastro de itens' })).toBeVisible();
  });
});
