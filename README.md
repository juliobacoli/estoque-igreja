# Estoque Igreja

Controle de estoque de itens de limpeza da igreja.

**Status:** Capítulo 1 — validação de infraestrutura. Ainda não há regra de negócio.

## Stack

- Backend: .NET 10 (ASP.NET Core, Controllers) + EF Core + Npgsql
- Frontend: Angular 22 (standalone, zoneless)
- Banco: PostgreSQL
- Deploy: Docker multi-estágio → Railway (build automático a cada push)

Frontend e backend rodam no mesmo container, servidos pela mesma origem — sem CORS.

## Estrutura

```
estoque-igreja/
├── Dockerfile
├── .dockerignore
└── source/
    ├── backend/EstoqueIgreja.Api/
    └── frontend/
```

## Rodar local

Backend (serve só a API, sem os arquivos do Angular):

```bash
export ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=estoque;Username=postgres;Password=postgres"
dotnet run --project source/backend/EstoqueIgreja.Api
```

Frontend em modo dev:

```bash
cd source/frontend
npm install
npm start
```

## Rodar o container completo

```bash
docker build -t estoque-igreja .
docker run -p 8080:8080 -e ConnectionStrings__DefaultConnection="..." estoque-igreja
```

## Endpoints

| Rota          | Descrição                                   |
| ------------- | ------------------------------------------- |
| `/`           | Frontend Angular                            |
| `/health`     | API de pé (200)                             |
| `/health/db`  | Conexão com o Postgres (200 ou 503)         |

## Configuração

A connection string **nunca** é commitada. É sempre injetada pela variável de
ambiente `ConnectionStrings__DefaultConnection` (duplo underscore). No Railway,
usar uma reference variable apontando para o serviço Postgres do projeto.

O `PORT` é injetado pelo Railway e lido pelo entrypoint do container. Sem ele,
o padrão é 8080.

## Critério de sucesso do Capítulo 1

- [ ] A URL pública carrega a tela do Angular
- [ ] A tela mostra a resposta de `/health` sem erro de CORS no console
- [ ] `/health/db` confirma conexão com o banco
- [ ] Recarregar em `/teste` não dá 404
