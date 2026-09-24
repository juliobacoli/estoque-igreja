# CLAUDE.md — Estoque ICPA

Contexto e regras do projeto para agentes de IA e desenvolvedores. Leia antes de alterar qualquer coisa.

---

## 1. O projeto

App web de controle de estoque dos itens de limpeza da igreja ICPA, feito para uso **no celular**. Os usuários não têm perfil técnico: textos simples e poucos passos importam.

| Perfil | Pode |
|---|---|
| **Admin** | Ver o dashboard, contar o estoque, ver o histórico, exportar PDF, cadastrar e remover itens |

**Módulos**: cada usuário acessa um ou mais módulos (`AcessoObreiros`, `AcessoAcaoSocial` em `Usuarios`). O login grava um claim `modulo` por acesso; os controllers de cada módulo exigem a policy de mesmo nome (`[Authorize(Policy = Politicas.Obreiros)]`). Usuário sem módulo não entra.

Deploy: um único container (front + API na mesma origem) publicado no Railway.

---

## 2. Regras obrigatórias

1. **Não alterar regra de negócio** em correção, refatoração ou melhoria, a menos que o pedido diga isso explicitamente. Isso inclui validações, permissões, cálculos, códigos HTTP e textos exibidos. **Na dúvida, pergunte antes.**
2. **Usar memória com eficiência** em correções e features novas, seguindo a seção 3.
3. **Testar na camada certa** toda mudança (seção 8). Merge na `main` só com o CI verde (seção 9).
4. **Escopo mínimo**: o menor diff que resolve o pedido. Melhorias opcionais vão separadas e só com aprovação.

---

## 3. Eficiência de memória

A API roda num container com pouca memória. O que já está em vigor, e **não deve ser revertido sem medição**:

| Onde | O quê | Por quê |
|---|---|---|
| `source/backend/EstoqueIgreja.Api/EstoqueIgreja.Api.csproj` | `ServerGarbageCollection=false` | O Server GC cria um heap por CPU; o Workstation GC atende uma API pequena com uma fração da memória. |
| `Dockerfile` (imagem final) | `mcr.microsoft.com/dotnet/aspnet:10.0-alpine` | Imagem cerca de 100 MB menor. O ICU é instalado (`icu-libs`) em vez de ligar `InvariantGlobalization`, que mudaria formatação e comparação por cultura. |
| `Dockerfile` | `DOTNET_GCHeapHardLimit=134217728` (128 MB) | Sem teto explícito, o GC deriva o limite do container e quase não compacta. |
| `source/backend/EstoqueIgreja.Infrastructure/DependencyInjection.cs` | `AddDbContextPool<AppDbContext>` | Reaproveita instâncias de `DbContext` em vez de criar uma por requisição. |
| Handlers de consulta | `AsNoTracking()` | O change tracker não guarda cópias de entidades que só são lidas. |

**Medições registradas** no commit `b48f332` (branch `perf/reduce-runtime-memory`, **não mergeada**):
- `gcConcurrent=0` foi testado e descartado: sem background GC, o RSS subiu 37%.
- Gerar um PDF (QuestPDF) custa cerca de 55 MB que não voltam: é o maior consumidor conhecido.

### Ao escrever código novo

- Consultas de leitura com `AsNoTracking()` e projeção só dos campos necessários (`Select` para um record de resultado).
- Paginar tudo que cresce com o tempo (como o histórico). Nunca carregar uma tabela inteira para filtrar em memória.
- Filtrar, ordenar e contar no banco (`Where`, `OrderBy`, `AnyAsync`, `MaxAsync`), não em listas.
- Serviços singleton não guardam estado que cresce (listas, caches sem limite).
- Mudança que afete memória: medir antes e depois e registrar o resultado na mensagem do commit.

---

## 4. Estrutura

```
estoque-igreja/
├── CLAUDE.md
├── Dockerfile                    # build do front + API e imagem final Alpine
├── .github/workflows/ci.yml      # CI: backend, frontend, e2e, docker
└── source/
    ├── backend/                  # .NET 10
    │   ├── EstoqueIgreja.Domain/
    │   ├── EstoqueIgreja.Application/
    │   ├── EstoqueIgreja.Infrastructure/
    │   ├── EstoqueIgreja.Api/
    │   ├── EstoqueIgreja.UnitTests/
    │   └── EstoqueIgreja.IntegrationTests/
    ├── frontend/                 # Angular 22
    └── e2e/                      # Playwright (ver source/e2e/README.md)
```

---

## 5. Padrão do backend

### Camadas e dependências

| Camada | Responsabilidade | Referencia |
|---|---|---|
| **Domain** | Entidades, enums e regras da própria entidade | nada |
| **Application** | Casos de uso (commands e queries), validação, interfaces e exceções | Domain |
| **Infrastructure** | EF Core (`AppDbContext`, migrations), BCrypt, geração de PDF | Application, Domain |
| **Api** | Controllers, middleware de erro, autenticação por cookie, `Program.cs` | todas |

### Casos de uso (MediatR)

Uma pasta por caso de uso, com os arquivos lado a lado:

```
Application/Itens/Commands/CriarItem/
├── CriarItemCommand.cs            # record da requisição + record do resultado
├── CriarItemCommandHandler.cs     # a lógica
└── CriarItemCommandValidator.cs   # FluentValidation
```

Os casos de uso da Ação Social ficam em `Application/AcaoSocial/` (tabelas `ItensSociais` e `MovimentacoesSociais`, rotas em `/api/acao-social`), separados dos Obreiros.

- **Controllers** só recebem a requisição, fazem `_mediator.Send(...)` e devolvem o resultado. Nada de lógica.
- **Validação** de entrada fica no `Validator`. O `ValidationBehavior` roda todos os validators antes do handler.
- **Erros de negócio** são exceções da Application, traduzidas pelo `TratamentoErroMiddleware`:
  - `ValidationException` → 400 com `{ error, errors }`
  - `ConflitoException` → 400 com `{ error }`
  - `NaoEncontradoException` → 404 com `{ error }`
  - qualquer outra → 500 com `{ error: "Erro interno" }`
- **Acesso a dados** pela interface `IAppDbContext`, nunca pelo `AppDbContext` concreto na Application.
- **Endpoints** exigem autenticação por padrão (FallbackPolicy). O que for público declara `[AllowAnonymous]`; o que for só de admin declara `[Authorize(Roles = "Admin")]`.

### Entidades

- Setters privados e construtor privado; criação por método de fábrica (`Item.Criar`, `Usuario.Criar`).
- Regras que dependem só da entidade ficam nela (`Item.AtualizarQuantidade`, `Item.Normalizar`).

### Estilo

- Nomes de classes, métodos e variáveis **em português**.
- Comentários explicam o **porquê** de uma decisão, não o que o código faz.
- `if` com uma única instrução **sem chaves**:

  ```csharp
  if (item is null)
      throw new NaoEncontradoException("Item não encontrado");
  ```

---

## 6. Padrão do frontend

- **Componentes standalone**, sem NgModules próprios. App **zoneless** (sem zone.js).
- **Estado** com `signal` e `computed`. Dependências com `inject()`.
- **Templates** com o control flow novo (`@if`, `@for`).
- **Pastas** em `src/app/`:

  | Pasta | Conteúdo |
  |---|---|
  | `core/` | Services HTTP, guards, interceptor, `models.ts` |
  | `paginas/` | Uma pasta por tela (`dashboard`, `atualizar-estoque`, `cadastro-item`, `historico`, `login`; da Ação Social: `acao-social`, `cadastro-item-social`) |
  | `shared/` | Diálogos e animações reutilizados |
  | `layout/` | Shell com o menu lateral |

- **Visual**: Angular Material, com a identidade da igreja em `src/styles.scss` (tokens `--roxo-*`, `--dourado`, `--erro`, classe `.glass`). Reaproveite os tokens em vez de criar cores novas.
- **Textos** em português, pensados para usuário não técnico.
- **Erros da API**: mostrar `resposta.error?.error` quando existir, com uma mensagem padrão como fallback.
- **Permissões na tela** (esconder menu, `adminGuard`) são só conveniência. A barreira real é o 403 da API.
- Em TypeScript, os `if` mantêm as chaves (padrão atual do front).

---

## 7. Rodar localmente

**Banco** (PostgreSQL local; em Development, a API usa `localhost:5432`, banco `estoque`, usuário e senha `postgres`):

```powershell
docker start estoque-pg
# ou, na primeira vez:
docker run -d --name estoque-pg -e POSTGRES_DB=estoque -e POSTGRES_PASSWORD=postgres -p 5432:5432 postgres:17-alpine
```

**API** (aplica as migrations na subida; `http://localhost:5297`):

```powershell
dotnet run --project source/backend/EstoqueIgreja.Api
```

**Front** (`http://localhost:4200`; o proxy envia `/api`, `/auth` e `/health` para a API):

```powershell
cd source/frontend
npm start
```

**Usuários**: as migrations **não criam usuários**. Para criar um num banco local, gere o hash BCrypt e insira:

```powershell
docker run --rm httpd:alpine htpasswd -nbBC 11 "" SUA_SENHA   # remova o ":" do início da saída
docker exec -it estoque-pg psql -U postgres -d estoque -c "INSERT INTO \"Usuarios\" (\"Id\",\"Login\",\"SenhaHash\",\"Perfil\",\"CriadoEm\",\"AcessoObreiros\",\"AcessoAcaoSocial\") VALUES (gen_random_uuid(),'admin','HASH_AQUI','Admin',now(),true,false);"
```

---

## 8. Testes

| Camada | Ferramentas | Onde | Comando |
|---|---|---|---|
| Backend unitário | xUnit, Moq, EF Core InMemory | `source/backend/EstoqueIgreja.UnitTests` | `dotnet test source/backend/EstoqueIgreja.UnitTests` |
| Backend integração | xUnit, WebApplicationFactory, Testcontainers (Postgres) | `source/backend/EstoqueIgreja.IntegrationTests` | `dotnet test source/backend/EstoqueIgreja.IntegrationTests` (Docker aberto) |
| Front unitário | Vitest + jsdom | `source/frontend/src/**/*.spec.ts` | `cd source/frontend` e `npm test` |
| E2E | Playwright (navegador real contra a imagem de produção) | `source/e2e` | Ver **[source/e2e/README.md](source/e2e/README.md)** |

### Qual teste acompanha cada mudança

| Mudança | Teste |
|---|---|
| Regra de entidade, handler, validator | Backend unitário |
| Endpoint, query SQL, permissão, transação, migration | Backend integração |
| Lógica de componente, service, guard | Front unitário |
| Fluxo que o usuário percorre de ponta a ponta | E2E |

### Regras

- Um teste descreve o comportamento esperado. **Se ele falha, corrija o código, não o teste**, a menos que a mudança de comportamento tenha sido pedida.
- Ao criar um teste, prove que ele pega falha: quebre o comportamento de propósito, veja falhar e desfaça.
- Um comportamento atual está **documentado por teste, sem correção** (não mudar sem pedido): quantidade negativa devolve **400 no formato ProblemDetails**, sem a propriedade `error` que o front lê.

---

## 9. CI e proteção da `main`

- `.github/workflows/ci.yml` roda em todo PR para a `main` e em todo push na `main`:

  | Job | O que faz |
  |---|---|
  | `backend` | build, testes unitários e de integração |
  | `frontend` | testes unitários e build de produção |
  | `e2e` | sobe a imagem de produção + Postgres e roda o Playwright |
  | `docker` | build da imagem de deploy (sem publicar) |

- Os **4 jobs são obrigatórios** para o merge, **inclusive para admin**. Não há exigência de revisão.

---

## 10. Convenções de git

- **Commits em inglês**, no padrão `tipo: descrição` (`feat`, `fix`, `test`, `ci`, `perf`, `docs`, `refactor`, `style`).
- **Descrição do PR em português**, mesmo com os commits em inglês.
- **Sem co-autor** nos commits e sem "Generated with…" nas descrições de PR.
- **Uma branch por assunto**, com prefixo igual ao tipo (`feat/...`, `fix/...`, `test/...`), a partir da `main` atualizada.
- **Commits separados por assunto** dentro do PR (ex.: estrutura, testes, CI).
- Nunca commitar direto na `main`: sempre PR.
- Mudança de memória ou performance: incluir no corpo do commit o que foi medido.

---

## 11. Pegadinhas conhecidas

- **Dockerfile restaura só a API** (`dotnet restore EstoqueIgreja.Api/EstoqueIgreja.Api.csproj`). Restaurar a `.sln` quebraria o build, porque os projetos de teste não são copiados para a imagem.
- **Imagem velha**: nos testes E2E, use `docker compose up -d --build --wait`. Sem `--build`, o Docker reaproveita a última imagem e o teste roda contra código antigo.
- **Portas reservadas no Windows**: o Hyper-V/WSL reserva faixas de portas (a `55432` já falhou). Veja com `netsh interface ipv4 show excludedportrange protocol=tcp`.
- **Migrations sem usuários**: um banco novo não tem login; é preciso inserir os usuários (seção 7).
- **Horário do PDF**: o container roda em UTC e o relatório converte para `America/Sao_Paulo`. Por isso a imagem precisa do pacote `tzdata`.
- **Versão no menu**: vem do `ARG RAILWAY_GIT_COMMIT_SHA` do `Dockerfile`, passado ao front com `--define VERSAO_APP`. Fora do Railway (CI, E2E, local) aparece `dev`.
- **Usuário novo precisa de módulo**: o `INSERT` manual (seção 7) tem de marcar `AcessoObreiros` e/ou `AcessoAcaoSocial`, senão o login responde "Usuário sem acesso".
