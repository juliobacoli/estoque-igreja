import { randomUUID } from 'node:crypto';
import { Client } from 'pg';

const CONEXAO = process.env.E2E_DATABASE_URL ?? 'postgres://postgres:postgres@localhost:15432/estoque';

// telaInicial: título da primeira tela depois do login, que o entrarComo espera aparecer.
export const USUARIOS = {
  admin: { login: 'admin', senha: 'e2e-admin-senha', perfil: 'Admin', obreiros: true, acaoSocial: false, telaInicial: 'Estoque atual' },
  social: { login: 'social', senha: 'e2e-admin-senha', perfil: 'Admin', obreiros: false, acaoSocial: true, telaInicial: 'Estoque da Ação Social' }
} as const;

export type Usuario = keyof typeof USUARIOS;

// Hash BCrypt da senha acima, a mesma nas duas contas. Só existe no banco descartável dos testes.
const HASH_SENHA = '$2y$10$iPwEkWrddebdl.x1Nr7GkO.QxyO6Bi0ytYjxAcFMMVVyg2ZTPpXS2';

async function comBanco<T>(acao: (cliente: Client) => Promise<T>): Promise<T> {
  const cliente = new Client({ connectionString: CONEXAO });
  await cliente.connect();

  try {
    return await acao(cliente);
  } finally {
    await cliente.end();
  }
}

/** Mesma normalização do backend (Item.Normalizar): sem acento, minúsculas, sem espaços nas pontas. */
function normalizar(nome: string) {
  return nome.trim().normalize('NFD').replace(/\p{Mn}/gu, '').normalize('NFC').toLowerCase();
}

/** Banco limpo, só com as contas de admin (Obreiros) e social (Ação Social). */
export function prepararBanco() {
  return comBanco(async (cliente) => {
    await cliente.query('TRUNCATE TABLE "MovimentacoesSociais", "ItensSociais", "AtualizacoesEstoque", "Itens", "Usuarios" CASCADE');

    for (const usuario of Object.keys(USUARIOS) as Usuario[]) {
      const { login, perfil, obreiros, acaoSocial } = USUARIOS[usuario];

      await cliente.query(
        `INSERT INTO "Usuarios" ("Id", "Login", "SenhaHash", "Perfil", "CriadoEm", "AcessoObreiros", "AcessoAcaoSocial")
         VALUES ($1, $2, $3, $4, now(), $5, $6)`,
        [randomUUID(), login, HASH_SENHA, perfil, obreiros, acaoSocial]
      );
    }
  });
}

/** Cadastra um item direto no banco, para os testes que não são sobre o cadastro. */
export function criarItem(nome: string, unidade = 'unidade', estoqueAtual = 0) {
  return comBanco(async (cliente) => {
    const id = randomUUID();

    await cliente.query(
      'INSERT INTO "Itens" ("Id", "Nome", "NomeNormalizado", "Unidade", "EstoqueAtual", "CriadoEm", "Ativo") VALUES ($1, $2, $3, $4, $5, now(), true)',
      [id, nome, normalizar(nome), unidade, estoqueAtual]
    );

    return id;
  });
}

/** Registra uma contagem antiga direto no banco (histórico e estoque), feita pelo admin. */
export function registrarContagem(itemId: string, quantidadeNova: number, diasAtras = 0) {
  return comBanco(async (cliente) => {
    await cliente.query(
      `INSERT INTO "AtualizacoesEstoque" ("Id", "ItemId", "QuantidadeAnterior", "QuantidadeNova", "UsuarioId", "Data")
       SELECT $1, i."Id", i."EstoqueAtual", $2, u."Id", now() - make_interval(days => $3)
       FROM "Itens" i, "Usuarios" u
       WHERE i."Id" = $4 AND u."Login" = 'admin'`,
      [randomUUID(), quantidadeNova, diasAtras, itemId]
    );

    await cliente.query('UPDATE "Itens" SET "EstoqueAtual" = $1 WHERE "Id" = $2', [quantidadeNova, itemId]);
  });
}
