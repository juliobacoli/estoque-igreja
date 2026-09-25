# Testes E2E (Playwright)

Guia prático para rodar, entender e escrever os testes de ponta a ponta do Estoque ICPA.

---

## 1. O que é e o que cobre

Um robô abre um **navegador de verdade** (Chromium emulando um celular Pixel 7), clica e digita como um usuário, contra o **app completo**: a imagem de produção (front + API) e um PostgreSQL real. Se a tela não mostrar o esperado, o teste falha.

Os testes de unidade e de integração verificam peças isoladas. O E2E verifica se **o fluxo inteiro funciona junto**.

| Arquivo | O que testa |
|---|---|
| `testes/acao-social.spec.ts` | Ação Social: cadastra item, registra doação em pacotes × kg e ajusta com motivo |
| `testes/autenticacao.spec.ts` | Login inválido, login válido, redirecionamento para o login sem sessão, logout |
| `testes/permissoes.spec.ts` | Admin vê o cadastro no menu e consegue abrir |
| `testes/cadastro.spec.ts` | Admin cadastra item e ele aparece no dashboard; remover item com histórico tira do dashboard e mantém no histórico |
| `testes/cesta.spec.ts` | Define a cesta, mostra quantas dá para montar e o que falta, monta e é barrado sem estoque |
| `testes/contagem.spec.ts` | Admin conta, salva, e o dashboard mostra a quantidade nova e o card "Estoque em dia"; sair sem salvar pede confirmação |
| `testes/historico.spec.ts` | Histórico mostra a contagem feita e filtra por item |
| `testes/modulos.spec.ts` | Módulo no cabeçalho, login e versão no menu; usuário da Ação Social não vê nem acessa os Obreiros |
| `testes/pdf.spec.ts` | Exportar baixa um PDF válido |

---

## 2. Pré-requisitos

- **Docker Desktop** aberto.
- **Node 24**.

Todos os comandos deste guia rodam **dentro da pasta `source/e2e`**.

---

## 3. Primeira vez na máquina

```powershell
cd source/e2e
npm ci                           # instala o Playwright e o cliente do Postgres
npx playwright install chromium  # baixa o navegador usado nos testes
```

---

## 4. Rodar os testes

```powershell
cd source/e2e

docker compose down -v                # 1. derruba qualquer ambiente anterior
docker compose up -d --build --wait   # 2. constrói a imagem do código atual e sobe app + banco
npx playwright test                   # 3. roda os testes
docker compose down -v                # 4. derruba tudo no final
```

- `--build` garante que a imagem corresponde ao código atual (veja a seção 7).
- `--wait` só libera o terminal quando app e banco estão prontos.

---

## 5. Formas de acompanhar

| Comando | Para quê |
|---|---|
| `npx playwright test` | Roda tudo no terminal e mostra ok/falha de cada teste |
| `npx playwright test --ui` | Abre a janela do Playwright para rodar e examinar passo a passo (**recomendado para entender**) |
| `npx playwright test --headed` | Mostra a janela do navegador enquanto roda |
| `npx playwright test testes/contagem.spec.ts --debug` | Um arquivo só, pausado, avançando linha a linha |
| `npx playwright test -g "sair com contagem"` | Só os testes cujo nome contém o texto |
| `npx playwright test --reporter=html` e depois `npx playwright show-report` | Gera e abre o relatório em HTML |

### Usando o modo UI (`--ui`)

1. O **▶** ao lado de **TESTS** roda todos. O **▶** ao lado de um teste roda só ele.
2. Para ver os detalhes, clique no **nome do teste**, não no nome do grupo (o grupo não tem passos próprios).
3. **Actions** lista cada passo (abrir página, preencher, clicar, conferir). Clique num passo para ver a tela naquele momento.
4. **Before / After** mostram a tela antes e depois da ação.
5. Abas de baixo:
   - **Errors**: a mensagem da falha.
   - **Source**: a linha do teste.
   - **Log**: o que o Playwright procurou e quanto tempo esperou.
   - **Network**: as chamadas feitas para a API.
6. Em **TESTING OPTIONS**, marque **Show browser** para ver o navegador abrindo de verdade.

Se as abas de baixo não aparecerem, maximize a janela ou arraste a divisória para cima.

---

## 6. Quando um teste falha

### Onde ver o erro

- **Terminal ou aba Errors**: mostra o seletor usado (`Locator`), o que era esperado (`Expected`) e o que aconteceu (ex.: `element(s) not found` depois de esperar 5 s).
- **Page snapshot**: a "fotografia" em texto do que estava na tela no momento da falha.
- **Screenshot e vídeo**: ficam em `test-results/` (só dos testes que falharam).
- **Copy prompt** (no relatório/UI): junta erro, snapshot e código do teste num texto pronto para colar numa IA.

### O problema está no app ou no teste?

1. **Primeiro, descarte ambiente velho**: rode de novo com `docker compose up -d --build --wait`. Sem `--build`, a imagem pode ser de uma versão antiga do código.
2. **Se continua falhando e ninguém pediu mudança de comportamento**: o teste descreve o que o app deveria fazer, e o app não fez. **Corrija o app, não o teste.**
3. **Se a mudança de comportamento ou de texto foi pedida**: atualize o teste no mesmo PR da mudança.

> Nunca altere um teste só para ele passar. Um teste ajustado para passar esconde o bug que ele deveria mostrar.

---

## 7. Como o ambiente funciona

### As duas imagens

| Imagem | De onde vem |
|---|---|
| `postgres:17-alpine` (banco) | Baixada automaticamente do Docker Hub na primeira execução. |
| `estoque-e2e-app` (front + API) | **Construída na sua máquina** a partir do `Dockerfile` da raiz do repositório. Durante o build, o Docker baixa as imagens-base (Node e .NET). |

- A **imagem do app não fica no git nem é baixada pronta**: o que fica no projeto é a receita (`Dockerfile`). Qualquer pessoa que clonar o repositório constrói a mesma imagem.
- Guardar a imagem pronta seria ruim: são centenas de MB e ela ficaria desatualizada. O teste precisa rodar contra o **código atual**.
- **Sempre use `--build`** depois de mudar o código (ou na dúvida). Sem ele, o Docker reaproveita a última imagem construída.
- As imagens ficam no Docker local: Docker Desktop → **Images**, ou `docker images`.

### Containers e portas

| Container | Porta na máquina | Variável para trocar |
|---|---|---|
| `estoque-e2e-app-1` | `5080` | `E2E_BASE_URL` (padrão `http://localhost:5080`) |
| `estoque-e2e-db-1` | `15432` | `E2E_DATABASE_URL` (padrão `postgres://postgres:postgres@localhost:15432/estoque`) |

O app roda em modo **Production**, igual ao deploy (o Chromium aceita o cookie `Secure` em `localhost`).

### Dados de teste

Antes de **cada** teste, o banco é esvaziado e recebe duas contas:

| Perfil | Login | Senha | Módulo |
|---|---|---|---|
| Admin | `admin` | `e2e-admin-senha` | Obreiros |
| Admin | `social` | `e2e-admin-senha` | Ação Social |

Essas contas só existem no banco descartável dos testes. Por usarem o mesmo banco, **os testes rodam um de cada vez**.

---

## 8. Como escrever um teste novo

1. Crie o arquivo em `testes/<assunto>.spec.ts`.
2. Importe `test` e `expect` de `../suporte/fixtures` (e **não** de `@playwright/test`). É isso que limpa o banco antes do teste.
3. **Prepare os dados pelo banco** (rápido) e **faça e confira as ações pela tela** (é o que o usuário faz).

### Helpers disponíveis

| Helper | Arquivo | O que faz |
|---|---|---|
| `entrarComo(page, 'admin' \| 'social')` | `suporte/fixtures.ts` | Faz login e espera a tela inicial do módulo |
| `preencherLogin(page, login, senha)` | `suporte/fixtures.ts` | Preenche e envia o login, sem esperar resultado |
| `abrirMenu(page)` | `suporte/fixtures.ts` | Abre o menu lateral |
| `criarItem(nome, unidade?, estoque?)` | `suporte/banco.ts` | Cadastra um item direto no banco e devolve o id |
| `registrarContagem(itemId, quantidade, diasAtras?)` | `suporte/banco.ts` | Grava uma contagem (histórico + estoque) feita pelo admin |

### Boas práticas

- **Encontre os elementos pelo que o usuário vê**: `getByRole('button', { name: 'Salvar contagem' })`, `getByLabel('Login', { exact: true })`. Evite classes CSS e ids.
- **Nome que contém outro nome**: `name` casa por trecho ("Remover" também casa com "Remover Vassoura"). Use `{ exact: true }` ou restrinja o escopo: `page.getByRole('dialog').getByRole('button', { name: 'Remover' })`.
- **Não use esperas fixas** (`waitForTimeout`). O `expect` do Playwright já espera o elemento aparecer.
- **Prove que o teste pega falha**: quebre o comportamento de propósito, rode, confirme que falhou e desfaça.

### Exemplo

```ts
import { criarItem } from '../suporte/banco';
import { entrarComo, expect, test } from '../suporte/fixtures';

test('admin vê o item cadastrado no dashboard', async ({ page }) => {
  await criarItem('Sabão', 'unidade', 3);

  await entrarComo(page, 'admin');

  await expect(page.getByRole('listitem').filter({ hasText: 'Sabão' })).toContainText('3');
});
```

---

## 9. No CI

- O job **`e2e`** (`.github/workflows/ci.yml`) roda em todo PR e em todo push na `main`, e é **obrigatório para o merge**.
- A cada execução, a imagem do app é **construída do zero** a partir do código do PR.
- Um teste que falha tem **uma nova tentativa** com trace. Se só passar na segunda, aparece como **flaky** no log: é instabilidade e deve ser investigada.
- Quando falha:
  - o passo **Logs do app** mostra o log da API;
  - o relatório completo (screenshots, vídeos, traces) fica em **Actions → run → Artifacts → `playwright-report`** por 7 dias. Baixe, extraia e abra com `npx playwright show-report <pasta-extraída>`.

---

## 10. Problemas comuns

| Sintoma | Causa provável | Solução |
|---|---|---|
| Teste falha, mas o código no git está correto | Imagem do app antiga | `docker compose up -d --build --wait` |
| `ports are not available` / `bind: ... proibida` | Porta ocupada ou reservada pelo Windows (Hyper-V/WSL) | Ver as faixas com `netsh interface ipv4 show excludedportrange protocol=tcp`, trocar a porta no `docker-compose.yml` e ajustar `E2E_BASE_URL` ou `E2E_DATABASE_URL` |
| `failed to connect to the docker API` | Docker Desktop fechado | Abrir o Docker Desktop |
| `Executable doesn't exist` | Chromium do Playwright não instalado | `npx playwright install chromium` |
| Todos os testes falham logo no login / `ECONNREFUSED` | Ambiente não subiu | `docker compose ps` e `docker compose logs app` |
