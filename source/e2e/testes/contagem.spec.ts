import { criarItem, registrarContagem } from '../suporte/banco';
import { abrirMenu, entrarComo, expect, test } from '../suporte/fixtures';

test.describe('Contagem de estoque', () => {
  test('voluntário conta, salva e o dashboard mostra a quantidade nova e o estoque em dia', async ({ page }) => {
    const sabao = await criarItem('Sabão', 'unidade');
    await registrarContagem(sabao, 3, 20);

    await entrarComo(page, 'voluntario');

    // Antes: contagem antiga, card de atraso.
    const card = page.getByRole('status').filter({ hasText: 'Estoque' });
    await expect(card).toContainText(/Estoque sem contagem há \d+ dias/);
    await expect(page.getByRole('listitem').filter({ hasText: 'Sabão' })).toContainText('3');

    await page.getByRole('button', { name: 'Atualizar estoque' }).click();
    await page.getByRole('button', { name: 'Aumentar Sabão' }).click();
    await page.getByRole('button', { name: 'Aumentar Sabão' }).click();
    await expect(page.getByLabel('Quantidade de Sabão')).toHaveValue('5');
    await page.getByRole('button', { name: 'Salvar contagem' }).click();

    // Depois: volta ao dashboard com a quantidade nova e o card em dia.
    await expect(page.getByText('Contagem salva.')).toBeVisible();
    await expect(page).toHaveURL(/\/dashboard$/);
    await expect(page.getByRole('listitem').filter({ hasText: 'Sabão' })).toContainText('5');
    await expect(card).toContainText('Estoque em dia');
    await expect(card).toContainText('Contado hoje.');
  });

  test('sair com contagem não salva pede confirmação', async ({ page }) => {
    await criarItem('Sabão', 'unidade', 3);

    await entrarComo(page, 'voluntario');
    await page.getByRole('button', { name: 'Atualizar estoque' }).click();
    await page.getByRole('button', { name: 'Aumentar Sabão' }).click();

    // Cancelar mantém a tela e a contagem.
    await abrirMenu(page);
    await page.getByRole('link', { name: 'Dashboard' }).click();
    const confirmacao = page.getByRole('dialog');
    await expect(confirmacao).toContainText('Sair sem salvar?');
    await confirmacao.getByRole('button', { name: 'Cancelar' }).click();

    await expect(page).toHaveURL(/\/atualizar$/);
    await expect(page.getByLabel('Quantidade de Sabão')).toHaveValue('4');

    // Confirmar sai sem gravar nada.
    await abrirMenu(page);
    await page.getByRole('link', { name: 'Dashboard' }).click();
    await page.getByRole('dialog').getByRole('button', { name: 'Sair sem salvar' }).click();

    await expect(page).toHaveURL(/\/dashboard$/);
    await expect(page.getByRole('listitem').filter({ hasText: 'Sabão' })).toContainText('3');
  });
});
