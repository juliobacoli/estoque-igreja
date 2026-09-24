import { Page } from '@playwright/test';
import { abrirMenu, entrarComo, expect, test } from '../suporte/fixtures';

/** Cadastra o item e registra a doação pela API, já logado; o teste é sobre a cesta. */
async function itemComEstoque(page: Page, nome: string, unidade: string, estoque: number) {
  const criado = await page.request.post('/api/acao-social/itens', { data: { nome, unidade } });
  const { id } = await criado.json();
  await page.request.post(`/api/acao-social/itens/${id}/entradas`, { data: { quantidade: estoque } });
}

test.describe('Cestas', () => {
  test('define a cesta, vê quantas dá para montar, monta e é barrado sem estoque', async ({ page }) => {
    await entrarComo(page, 'social');
    await itemComEstoque(page, 'Arroz', 'kg', 12);
    await itemComEstoque(page, 'Óleo', 'garrafa', 7);

    await abrirMenu(page);
    await page.getByRole('link', { name: 'Cestas' }).click();
    await expect(page.getByText('A cesta ainda não foi definida.')).toBeVisible();

    // Define o modelo: 5 kg de arroz e 1 óleo por cesta.
    await page.getByRole('link', { name: 'Definir a cesta' }).click();
    await page.getByLabel('Quantidade de Arroz por cesta').fill('5');
    await page.getByLabel('Quantidade de Óleo por cesta').fill('1');
    await page.getByRole('button', { name: 'Salvar cesta' }).click();

    await expect(page).toHaveURL(/\/acao-social\/cesta$/);
    await expect(page.locator('.numero').filter({ hasText: 'dá para montar' })).toContainText('2');
    await expect(page.getByText('Arroz: 3 kg')).toBeVisible();

    // Monta 2: o arroz vai de 12 para 2 e as prontas vão para 2.
    await page.getByLabel('Quantas cestas vai montar?').fill('2');
    await page.getByRole('button', { name: 'Montar cestas' }).click();
    await expect(page.getByText('2 cestas montadas. Prontas: 2.')).toBeVisible();
    await expect(page.locator('.numero').filter({ hasText: 'cestas prontas' })).toContainText('2');
    await expect(page.locator('.numero').filter({ hasText: 'dá para montar' })).toContainText('0');
    await expect(page.getByRole('button', { name: 'Montar cestas' })).toBeDisabled();

    // Sem arroz para mais uma, a API recusa e explica o que falta.
    await page.request.post('/api/acao-social/cesta/montagens', { data: { quantidade: 1 } }).then(async (resposta) => {
      expect(resposta.status()).toBe(400);
      expect((await resposta.json()).error).toBe(
        'Não dá para montar 1 cesta. Falta: Arroz (tem 2, precisa de 5). Não dá para montar nenhuma agora.'
      );
    });
  });
});
