import { criarItem, registrarContagem } from '../suporte/banco';
import { abrirMenu, entrarComo, expect, test } from '../suporte/fixtures';

test.describe('Histórico', () => {
  test('mostra a contagem feita e filtra por item', async ({ page }) => {
    await criarItem('Sabão', 'unidade');
    const esponja = await criarItem('Esponja', 'pacote');
    await registrarContagem(esponja, 2, 2);

    // Contagem feita agora, pela tela, pelo admin.
    await entrarComo(page, 'admin');
    await page.getByRole('button', { name: 'Atualizar estoque' }).click();
    await page.getByRole('button', { name: 'Aumentar Sabão' }).click();
    await page.getByRole('button', { name: 'Salvar contagem' }).click();
    await expect(page).toHaveURL(/\/dashboard$/);

    await abrirMenu(page);
    await page.getByRole('link', { name: 'Histórico' }).click();

    const registroSabao = page.getByRole('listitem').filter({ hasText: 'Sabão' });
    const registroEsponja = page.getByRole('listitem').filter({ hasText: 'Esponja' });
    await expect(registroSabao).toContainText('0 → 1');
    await expect(registroSabao).toContainText('Admin');
    await expect(registroEsponja).toContainText('0 → 2');

    await page.getByRole('combobox', { name: 'Filtrar por item' }).click();
    await page.getByRole('option', { name: 'Esponja' }).click();

    await expect(registroEsponja).toBeVisible();
    await expect(registroSabao).toHaveCount(0);
  });
});
