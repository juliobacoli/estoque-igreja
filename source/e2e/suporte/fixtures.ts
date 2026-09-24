import { expect, Page, test as base } from '@playwright/test';
import { prepararBanco, Usuario, USUARIOS } from './banco';

/** Todo teste começa com o banco limpo, sem precisar pedir. */
export const test = base.extend<{ bancoLimpo: void }>({
  bancoLimpo: [
    async ({}, use) => {
      await prepararBanco();
      await use();
    },
    { auto: true }
  ]
});

export { expect };

export async function preencherLogin(page: Page, login: string, senha: string) {
  await page.goto('/login');
  await page.getByLabel('Login', { exact: true }).fill(login);
  await page.getByLabel('Senha', { exact: true }).fill(senha);
  await page.getByRole('button', { name: 'Entrar' }).click();
}

export async function entrarComo(page: Page, usuario: Usuario) {
  await preencherLogin(page, USUARIOS[usuario].login, USUARIOS[usuario].senha);
  await expect(page.getByRole('heading', { name: USUARIOS[usuario].telaInicial })).toBeVisible();
}

export async function abrirMenu(page: Page) {
  await page.getByRole('button', { name: 'Abrir menu' }).click();
}
