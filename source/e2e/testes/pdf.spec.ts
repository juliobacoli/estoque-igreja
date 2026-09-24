import { readFile } from 'node:fs/promises';
import { criarItem } from '../suporte/banco';
import { entrarComo, expect, test } from '../suporte/fixtures';

test.describe('Exportar PDF', () => {
  test('exportar baixa o PDF do estoque', async ({ page }) => {
    // Simula um aparelho sem compartilhamento de arquivos: o app cai no download.
    // Sem isso, o Chromium no Windows abriria o diálogo de compartilhar do sistema.
    await page.addInitScript(() => {
      Object.defineProperty(navigator, 'canShare', { value: undefined });
      Object.defineProperty(navigator, 'share', { value: undefined });
    });
    await criarItem('Sabão', 'unidade', 3);

    await entrarComo(page, 'admin');

    const download = page.waitForEvent('download');
    await page.getByRole('button', { name: 'Exportar PDF' }).click();
    const arquivo = await download;

    expect(arquivo.suggestedFilename()).toBe('estoque.pdf');
    const conteudo = await readFile(await arquivo.path());
    expect(conteudo.subarray(0, 4).toString()).toBe('%PDF');
    await expect(page.getByText('PDF baixado — este aparelho não permite compartilhar arquivos.')).toBeVisible();
  });
});
