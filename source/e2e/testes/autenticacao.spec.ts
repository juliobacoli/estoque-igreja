import { abrirMenu, entrarComo, expect, preencherLogin, test } from '../suporte/fixtures';

test.describe('Autenticação', () => {
  test('login inválido mostra erro e continua na tela de login', async ({ page }) => {
    await preencherLogin(page, 'admin', 'senha-errada');

    await expect(page.getByRole('alert')).toHaveText('Login ou senha inválidos');
    await expect(page).toHaveURL(/\/login$/);
  });

  test('login válido abre o dashboard', async ({ page }) => {
    await entrarComo(page, 'admin');

    await expect(page).toHaveURL(/\/dashboard$/);
  });

  test('sem login, acessar o dashboard manda para o login', async ({ page }) => {
    await page.goto('/dashboard');

    await expect(page).toHaveURL(/\/login$/);
    await expect(page.getByRole('button', { name: 'Entrar' })).toBeVisible();
  });

  test('sair volta para o login e bloqueia as telas', async ({ page }) => {
    await entrarComo(page, 'admin');

    await abrirMenu(page);
    await page.getByRole('button', { name: 'Sair' }).click();
    await expect(page).toHaveURL(/\/login$/);

    await page.goto('/dashboard');
    await expect(page).toHaveURL(/\/login$/);
  });
});
