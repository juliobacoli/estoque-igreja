# Estoque Igreja

Controle de estoque dos itens de limpeza da igreja ICPA, feito para uso no celular.

- **Admins** contam o estoque, acompanham o dashboard e o histórico, exportam um PDF para compartilhar e cadastram e removem itens.
- O dashboard mostra há quantos dias o estoque não é contado, para lembrar o pessoal de atualizar.

> Regras do projeto, padrões de código e convenções de trabalho: **[CLAUDE.md](CLAUDE.md)**.

## Stack

- **Backend:** .NET 10 (ASP.NET Core, Controllers) + EF Core + Npgsql, MediatR e FluentValidation
- **Frontend:** Angular 22 (standalone, zoneless) + Angular Material
- **Banco:** PostgreSQL
- **Testes:** xUnit, Testcontainers, Vitest e Playwright
- **Deploy:** Docker multi-estágio → Railway (build automático a cada push)

Frontend e backend rodam no mesmo container, servidos pela mesma origem, sem CORS.

## Estrutura

```
estoque-igreja/
├── CLAUDE.md                     # regras e padrões do projeto
├── Dockerfile                    # imagem de produção (front + API)
├── .github/workflows/ci.yml      # CI
└── source/
    ├── backend/                  # Domain, Application, Infrastructure, Api + testes
    ├── frontend/                 # Angular
    └── e2e/                      # testes de ponta a ponta (Playwright)
```

## Rodar local

**1. Banco** (em Development, a API usa `localhost:5432`, banco `estoque`, usuário e senha `postgres`):

```bash
docker run -d --name estoque-pg -e POSTGRES_DB=estoque -e POSTGRES_PASSWORD=postgres -p 5432:5432 postgres:17-alpine
```

**2. Backend** (aplica as migrations ao subir; serve só a API em `http://localhost:5297`):

```bash
dotnet run --project source/backend/EstoqueIgreja.Api
```

**3. Frontend** (`http://localhost:4200`; o proxy encaminha `/api`, `/auth` e `/health` para a API):

```bash
cd source/frontend
npm install
npm start
```

**4. Usuários:** as migrations não criam contas. Como inserir um usuário no banco local está em [CLAUDE.md, seção 7](CLAUDE.md#7-rodar-localmente).

## Rodar o container completo

```bash
docker build -t estoque-igreja .
docker run -p 8080:8080 -e ConnectionStrings__DefaultConnection="..." estoque-igreja
```

## Testes

| Camada | Comando |
|---|---|
| Backend unitário | `dotnet test source/backend/EstoqueIgreja.UnitTests` |
| Backend integração (precisa do Docker) | `dotnet test source/backend/EstoqueIgreja.IntegrationTests` |
| Frontend unitário | `cd source/frontend && npm test` |
| E2E (navegador real contra a imagem de produção) | Guia completo em **[source/e2e/README.md](source/e2e/README.md)** |

## CI

Todo PR para a `main` e todo push na `main` rodam os jobs `backend`, `frontend`, `e2e` e `docker` (`.github/workflows/ci.yml`). Os quatro são obrigatórios: o merge só é liberado com tudo verde.

## Rotas

### Telas

| Rota | Tela | Acesso |
|---|---|---|
| `/login` | Login | Público |
| `/dashboard` | Estoque atual e status da contagem | Logado |
| `/atualizar` | Contagem de estoque | Logado |
| `/historico` | Histórico de contagens | Logado |
| `/cadastro` | Cadastro e remoção de itens | Admin |

### API

| Método e rota | Descrição | Acesso |
|---|---|---|
| `POST /auth/login` | Login (cookie de sessão) | Público |
| `POST /auth/logout` | Logout | Logado |
| `GET /auth/me` | Perfil da sessão atual | Logado |
| `GET /api/itens` | Itens ativos (`?incluirInativos=true` inclui os removidos) | Logado |
| `POST /api/itens` | Cadastra item | Admin |
| `PUT /api/itens/{id}/estoque` | Atualiza a quantidade e grava no histórico | Logado |
| `DELETE /api/itens/{id}` | Remove item (inativa se já tiver histórico) | Admin |
| `GET /api/historico` | Histórico paginado (`pagina`, `tamanhoPagina`, `itemId`) | Logado |
| `GET /api/estoque/exportar-pdf` | PDF do estoque atual | Logado |
| `GET /api/estoque/ultima-atualizacao` | Data da contagem mais recente | Logado |
| `GET /health` | API de pé (200) | Público |
| `GET /health/db` | Conexão com o Postgres (200 ou 503) | Público |

## Configuração

A connection string **nunca** é commitada. É sempre injetada pela variável de
ambiente `ConnectionStrings__DefaultConnection` (duplo underscore). No Railway,
usar uma reference variable apontando para o serviço Postgres do projeto.

O `PORT` é injetado pelo Railway e lido pelo entrypoint do container. Sem ele,
o padrão é 8080.
