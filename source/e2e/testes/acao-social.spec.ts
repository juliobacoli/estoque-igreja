import { abrirMenu, entrarComo, expect, test } from '../suporte/fixtures';

test.describe('Ação Social', () => {
  test('cadastra um item, registra doação e ajusta o estoque', async ({ page }) => {
    await entrarComo(page, 'social');
    await expect(page.getByText('Nenhum item cadastrado ainda.')).toBeVisible();

    // Cadastro
    await abrirMenu(page);
    await page.getByRole('link', { name: 'Cadastro de Itens' }).click();
    await page.getByLabel('Nome', { exact: true }).fill('Arroz');
    await page.getByLabel('Unidade', { exact: true }).click();
    await page.getByRole('option', { name: 'kg' }).click();
    await page.getByRole('button', { name: 'Cadastrar item' }).click();
    await expect(page.getByText('"Arroz" cadastrado.')).toBeVisible();

    // Doação
    await abrirMenu(page);
    await page.getByRole('link', { name: 'Estoque da Ação Social' }).click();
    const arroz = page.getByRole('listitem').filter({ hasText: 'Arroz' });
    await expect(arroz).toContainText('0 kg');

    await page.getByRole('button', { name: 'Registrar doação de Arroz' }).click();
    const dialogo = page.getByRole('dialog');
    await expect(dialogo.getByLabel('Quem doou')).toHaveCount(0);
    await dialogo.getByLabel('Pacotes').fill('2');
    await dialogo.getByLabel('Kg de cada pacote').fill('5');
    await expect(dialogo.getByText('Entra +10 kg')).toBeVisible();
    await dialogo.getByRole('button', { name: 'Salvar' }).click();

    await expect(page.getByText('Arroz: agora 10 kg.')).toBeVisible();
    await expect(arroz).toContainText('10 kg');

    // Ajuste: sem motivo o botão fica desabilitado
    await page.getByRole('button', { name: 'Ajustar Arroz' }).click();
    await expect(dialogo.getByLabel('Quantidade que existe agora')).toHaveValue('10');
    await dialogo.getByLabel('Quantidade que existe agora').fill('7');
    await expect(dialogo.getByRole('button', { name: 'Salvar' })).toBeDisabled();
    await dialogo.getByLabel('Motivo').fill('3 kg estragaram');
    await dialogo.getByRole('button', { name: 'Salvar' }).click();

    await expect(page.getByText('Arroz: agora 7 kg.')).toBeVisible();
    await expect(arroz).toContainText('7 kg');
  });
});
