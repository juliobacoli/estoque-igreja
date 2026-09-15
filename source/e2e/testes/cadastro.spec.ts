import { criarItem, registrarContagem } from '../suporte/banco';
import { abrirMenu, entrarComo, expect, test } from '../suporte/fixtures';

test.describe('Cadastro de itens', () => {
  test('admin cadastra um item e ele aparece no dashboard', async ({ page }) => {
    await entrarComo(page, 'admin');
    await abrirMenu(page);
    await page.getByRole('link', { name: 'Cadastro de Itens' }).click();

    await page.getByLabel('Nome', { exact: true }).fill('Rodo');
    await page.getByRole('combobox', { name: 'Unidade' }).click();
    await page.getByRole('option', { name: 'unidade' }).click();
    await page.getByRole('button', { name: 'Cadastrar item' }).click();

    await expect(page.getByText('"Rodo" cadastrado.')).toBeVisible();
    await expect(page.getByRole('listitem').filter({ hasText: 'Rodo' })).toContainText('0 em estoque');

    await abrirMenu(page);
    await page.getByRole('link', { name: 'Dashboard' }).click();
    await expect(page.getByRole('listitem').filter({ hasText: 'Rodo' })).toBeVisible();
  });

  test('remover item com histórico tira do dashboard e mantém no histórico', async ({ page }) => {
    const vassoura = await criarItem('Vassoura');
    await registrarContagem(vassoura, 2, 1);

    await entrarComo(page, 'admin');
    await abrirMenu(page);
    await page.getByRole('link', { name: 'Cadastro de Itens' }).click();

    await page.getByRole('button', { name: 'Remover Vassoura' }).click();
    await page.getByRole('dialog').getByRole('button', { name: 'Remover' }).click();
    await expect(page.getByText('Item removido da lista — o histórico foi mantido.')).toBeVisible();

    await abrirMenu(page);
    await page.getByRole('link', { name: 'Dashboard' }).click();
    await expect(page.getByRole('heading', { name: 'Estoque atual' })).toBeVisible();
    await expect(page.getByRole('listitem').filter({ hasText: 'Vassoura' })).toHaveCount(0);

    await abrirMenu(page);
    await page.getByRole('link', { name: 'Histórico' }).click();
    await expect(page.getByRole('listitem').filter({ hasText: 'Vassoura' })).toBeVisible();
  });
});
