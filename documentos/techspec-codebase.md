# Tech Spec: SportHub API — Codebase v5.0

> Última atualização: 2026-03-14
> Gerado automaticamente a partir da análise completa do codebase

---

## 1. Visão Geral

**SportHub** é uma API REST multi-tenant para gestão de estabelecimentos esportivos (quadras, reservas, esportes). Permite que cada estabelecimento (tenant) opere com dados isolados em um schema PostgreSQL próprio, acessível via subdomínio (`arena1.sporthub.app`) ou header `X-Tenant-Slug`.

O sistema é construído com **.NET 10 (Preview)** usando **Minimal APIs** e segue **Clean Architecture** com 4 camadas bem definidas: Domain, Application, Infrastructure e Api. Implementa CQRS via MediatR, Result Pattern via FluentResults, validação via FluentValidation e autenticação JWT Bearer.

Existe um papel de **SuperAdmin** (operador da plataforma) que gerencia tenants globalmente, e papéis por tenant (User, EstablishmentMember, Admin) para controle de acesso local.

---

## 2. Stack Tecnológico

| Categoria | Tecnologia | Versão |
|---|---|---|
| **Runtime** | .NET | 10.0 (Preview) |
| **Framework** | ASP.NET Core Minimal APIs | 10.0 |
| **ORM** | Entity Framework Core | 9.0.7 |
| **Banco de Dados** | PostgreSQL | 16 |
| **Cache** | Redis (StackExchange) | 7 |
| **CQRS/Mediator** | MediatR | 13.0.0 |
| **Result Pattern** | FluentResults | 4.0.0 |
| **Validação** | FluentValidation | 12.0.0 |
| **Autenticação** | JWT Bearer (HMAC-SHA256) | — |
| **Logging** | Serilog | 9.0.0 |
| **Documentação API** | Scalar (OpenAPI) | 2.6.1 |
| **Senhas** | PBKDF2 (SHA256, 10000 iter) | — |
| **Containerização** | Docker Compose | 3.9 |

### Dependências Chave
- `Npgsql.EntityFrameworkCore.PostgreSQL` 9.0.4
- `System.IdentityModel.Tokens.Jwt` 8.13.0
- `Microsoft.Extensions.Caching.StackExchangeRedis` 9.0.7
- `FluentValidation.AspNetCore` 11.3.1

---

## 3. Arquitetura e Padrões

### 3.1. Estrutura de Camadas (Clean Architecture)

```
SportHub.sln
├── src/
│   ├── SportHub.Domain          ← Entidades, Value Objects, Enums (zero dependências)
│   ├── SportHub.Application     ← CQRS, UseCases, Interfaces, Services, Validators
│   ├── SportHub.Infrastructure  ← EF Core, Repositórios, Security, Middleware de Tenant
│   └── SportHub.Api             ← Endpoints (Minimal APIs), DI, Middleware HTTP, Program.cs
└── tests/
    └── SportHub.Tests
```

**Direção de dependência:**
```
Api → Application, Infrastructure
Infrastructure → Application, Domain
Application → Domain
Domain → (nenhuma)
```

### 3.2. Padrões Predominantes

| Módulo/Diretório | Padrão Arquitetural | Notas |
|---|---|---|
| `SportHub.Domain` | Domain Model (Anêmico/Rico misto) | Tenant tem Factory Method (`Create`), User é anêmico |
| `SportHub.Application/UseCases` | CQRS (Command/Query Separation) | Via MediatR com `ICommand`/`IQuery` customizados |
| `SportHub.Application/Common/Errors` | Result Pattern (Typed Errors) | FluentResults com `Error` tipados (metadata `StatusCode`) |
| `SportHub.Infrastructure/Repositories` | Repository Pattern + Generic Base | `BaseRepository<T>` com `IEntity` constraint |
| `SportHub.Infrastructure/Persistence` | Multi-Tenant Schema-per-Tenant | `ApplicationDbContext` (por tenant) + `TenantDbContext` (global) |
| `SportHub.Api/Endpoints` | Minimal API Endpoints | Extension methods estáticos (`MapXxxEndpoints`) |
| `SportHub.Api/Behaviors` | Pipeline Behaviors (MediatR) | `ValidationBehavior` + `LoggingBehavior` |

### 3.3. Multi-Tenancy (Arquitetura Core)

A multi-tenancy é a arquitetura central do sistema:

1. **Schema-per-Tenant**: cada tenant tem um schema PostgreSQL isolado (`tenant_{slug}`)
2. **Schema público (`public`)**: contém a tabela `Tenants` (metadados globais) + tabelas `Users` do SuperAdmin
3. **Dois DbContexts**:
   - `TenantDbContext` → opera SEMPRE no schema `public` (tabela `Tenants`)
   - `ApplicationDbContext` → opera no schema dinâmico do tenant resolvido
4. **Resolução de Tenant** (pipeline):
   ```
   Request → TenantResolutionMiddleware → extrai slug (subdomínio ou X-Tenant-Slug)
           → busca no Redis/DB → preenche ITenantContext (Scoped)
           → ApplicationDbContext.OnModelCreating → HasDefaultSchema(tenantContext.Schema)
   ```
5. **TenantModelCacheKeyFactory**: gera chave de cache por schema (`TenantModelCacheKey` inclui `_schema`), garantindo que o EF Core compile modelo separado por tenant
6. **Provisioning**: `TenantProvisioningService` cria schema, executa migrations e faz seed

#### Rotas Bypass (sem tenant):
- `/api/tenants/**` — gestão de tenants (SuperAdmin)
- `/health`, `/scalar/**`, `/openapi/**`
- `/auth/superadmin`

#### Rotas Optional Tenant:
- `/auth/login` — permite login sem tenant (SuperAdmin)
- `/api/branding` — retorna branding se tenant existir

### 3.4. CQRS Customizado

Interfaces proprietárias sobre MediatR:

```csharp
// Commands (escrita)
ICommand : IRequest<Result>
ICommand<TResponse> : IRequest<Result<TResponse>>
ICommandHandler<TCmd> : IRequestHandler<TCmd, Result>
ICommandHandler<TCmd, TResp> : IRequestHandler<TCmd, Result<TResp>>

// Queries (leitura)
IQuery<TResponse> : IRequest<Result<TResponse>>
IQueryHandler<TQuery, TResp> : IRequestHandler<TQuery, Result<TResp>>
```

**Todos os handlers retornam `Result` ou `Result<T>`** (FluentResults), nunca lançam exceções para erros de negócio.

### 3.5. Pipeline Behaviors (MediatR)

Ordem de execução no pipeline:

1. **`LoggingBehavior`** (Scoped) — loga request/response com tempo de execução
2. **`ValidationBehavior`** (Transient) — executa todos os `IValidator<TRequest>`, lança `ValidationException` se falhar

---

## 4. Design de Código e Convenções

### 4.1. Nomenclatura

| Elemento | Padrão | Exemplo |
|---|---|---|
| **Entidades** | PascalCase, classe concreta | `User`, `Court`, `Tenant` |
| **Interfaces** | Prefixo `I` | `IEntity`, `ICacheService`, `IUsersRepository` |
| **Commands** | `{Ação}{Entidade}Command` | `RegisterUserCommand`, `CreateCourtCommand` |
| **Queries** | `{Ação}{Entidade}Query` | `GetAllTenantsQuery`, `GetAvailabilityQuery` |
| **Handlers** | `{Ação}{Entidade}Handler` | `RegisterUserHandler`, `LoginHandler` |
| **Validators** | `{Ação}{Entidade}Validator` | `RegisterUserValidator`, `LoginValidator` |
| **Responses** | `{Contexto}Response` | `AuthResponse`, `ProvisionTenantResponse` |
| **Endpoints** | `{Entidade}Endpoints` (static class) | `AuthEndpoints`, `TenantEndpoints` |
| **Repositórios** | `{Entidade}Repository` : `BaseRepository<T>` | `UsersRepository`, `CourtsRepository` |
| **Configurations EF** | `{Entidade}Configuration` | `UserConfiguration`, `TenantConfiguration` |
| **Settings** | `{Contexto}Settings` | `JwtSettings`, `AdminUserSettings` |
| **Errors** | Classe por tipo HTTP | `BadRequest`, `NotFound`, `Conflict`, `Unauthorized` |

### 4.2. Organização de UseCases

Cada UseCase fica em pasta própria: `UseCases/{Domínio}/{Ação}/`

```
UseCases/
├── Auth/
│   ├── AuthResponse.cs          ← DTO compartilhado
│   ├── Login/
│   │   ├── LoginCommand.cs
│   │   ├── LoginHandler.cs
│   │   └── LoginValidator.cs
│   ├── RegisterUser/
│   │   ├── RegisterUserCommand.cs
│   │   ├── RegisterUserHandler.cs
│   │   └── RegisterUserValidator.cs
│   └── RefreshToken/
├── Tenant/
│   ├── ProvisionTenant/
│   ├── GetAllTenants/
│   └── ...
├── Court/
├── Sport/
└── Reservation/
```

### 4.3. Tratamento de Erros

**Camada Application (Result Pattern):**

Erros tipados estendem `FluentResults.Error` com metadata `StatusCode`:

| Classe | StatusCode HTTP | Uso |
|---|---|---|
| `BadRequest` | **422** (não 400!) | Validação de negócio |
| `NotFound` | 404 | Recurso não encontrado |
| `Unauthorized` | 401 | Credenciais inválidas |
| `Forbidden` | 403 | Sem permissão |
| `Conflict` | 409 | Duplicidade (email, slug) |
| `InternalServerError` | 500 | Erro inesperado |

> ⚠️ **Gotcha**: `BadRequest` retorna **422**, não 400. O 400 real só ocorre em `DbUpdateException` com `UniqueViolation`.

**Camada Api (Exception Handler):**

`CustomExceptionHandler` captura exceções não tratadas:
- `ValidationException` (FluentValidation) → 422 com ProblemDetails + lista de erros
- `DbUpdateException` com `UniqueViolation` (PostgreSQL) → 400
- Qualquer outra → 500

`CustomAuthorizationMiddlewareResultHandler` garante respostas JSON padronizadas:
- Challenge (sem token) → 401 ProblemDetails
- Forbidden (sem role) → 403 ProblemDetails

**Conversão Result → IResult:**

`ResultExtensions.ToIResult()` converte `FluentResults.Result` para `Microsoft.AspNetCore.Http.IResult`:
- Success → 200/201/204 (configurável)
- Failure → ProblemDetails com statusCode extraído da metadata do primeiro Error

### 4.4. Padrão de Resposta da API

Todas as respostas de erro seguem **RFC 9457 (Problem Details)**:

```json
{
  "type": "https://httpstatuses.io/{status}",
  "title": "Operation failed",
  "detail": "E-mail 'x@y.com' is already in use.",
  "status": 409,
  "instance": "/auth/register",
  "traceId": "0HN...",
  "errors": [{ "message": "...", "metadata": { "StatusCode": 409 } }]
}
```

---

## 5. Autenticação e Autorização

### 5.1. JWT Bearer

- **Algoritmo**: HMAC-SHA256
- **Expiração Access Token**: configurável via `JwtSettings.ExpiryMinutes` (fallback: 120min/2h se não configurado ou ≤ 0)
- **Expiração Refresh Token**: 7 dias (hardcoded em `GenerateRefreshToken`)
- **Claims**: `NameIdentifier` (userId), `Email`, `Name` (fullName), `Role`
- **Configuração** (`appsettings.json`): `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience`, `Jwt:ExpiryMinutes` (padrão: 60)

### 5.2. Authorization Policies

| Policy | Requirement | Handler |
|---|---|---|
| `IsStaff` | `GlobalRoleRequirement(Staff)` | `GlobalRoleHandler` |
| `IsManager` | `GlobalRoleRequirement(Manager)` | `GlobalRoleHandler` |
| `IsOwner` | `GlobalRoleRequirement(Owner)` | `GlobalRoleHandler` |
| `IsSuperAdmin` | `SuperAdminRequirement` | `SuperAdminHandler` |

**Roles do Sistema (`UserRole` enum):**
- `User` (0) — usuário padrão
- `EstablishmentMember` (1) — membro do tenant
- `Admin` (2) — administrador do tenant
- `SuperAdmin` (99) — operador da plataforma

**Roles de Estabelecimento (`EstablishmentRole` enum):**
- `Staff` (0), `Manager` (1), `Owner` (2)

### 5.3. Senhas

`PasswordService` usa **PBKDF2** com SHA256:
- Salt: 32 bytes aleatórios (Base64)
- Hash: 64 bytes
- Iterações: 10.000

---

## 6. Modelo de Dados

### 6.1. Schema `public` (Global)

| Tabela | Descrição |
|---|---|
| `Tenants` | Metadados de cada tenant (slug, nome, status, branding, owner) |
| `Users` | Apenas o SuperAdmin reside aqui (via seed no startup) |

### 6.2. Schema `tenant_{slug}` (Por Tenant)

| Tabela | Descrição |
|---|---|
| `Users` | Usuários do tenant (Admin, Members, Users) |
| `Courts` | Quadras esportivas do tenant |
| `Sports` | Esportes disponíveis (many-to-many com Courts) |
| `Reservations` | Reservas de quadras (FK: Court, User) |

### 6.3. Entidades e Herança

```
AuditEntity (abstract)
├── User : AuditEntity, IEntity
├── Court : AuditEntity, IEntity
├── Sport : AuditEntity, IEntity
└── Reservation : AuditEntity, IEntity

Tenant (standalone, SEM IEntity, SEM AuditEntity)
```

**`AuditEntity`** fornece: `CreatedAt`, `UpdatedAt`, `CreatedBy`, `UpdatedBy`, `IsDeleted`, `DeletedBy`, `DeletedAt`
- Soft Delete automático via `ApplicationDbContext.SaveChangesAsync` (intercepta `EntityState.Deleted` → `MarkAsDeleted`)
- Restauração via `Restore()`

**`IEntity`** define apenas `Guid Id { get; }`

> ⚠️ **Gotcha**: `Tenant` NÃO implementa `IEntity` nem herda `AuditEntity`. Por isso, `TenantRepository` não herda `BaseRepository<T>` e opera via `TenantDbContext` separado.

### 6.4. Value Objects

- **`Address`** — Street, Number, Complement, Neighborhood, City, State, ZipCode (não utilizado atualmente nas entidades)

---

## 7. Camada de Infraestrutura

### 7.1. Repositórios

`BaseRepository<T>` (genérico, requer `IEntity`):
- `GetByIdAsync`, `GetAllAsync`, `AddAsync`, `UpdateAsync`, `RemoveAsync`
- `GetByIdsAsync`, `ExistsAsync`, `Query()`, `AddManyAsync`

> ✅ **Unit of Work**: `BaseRepository` **não** chama `SaveChangesAsync` — apenas adiciona/atualiza/remove no `DbSet`. O handler é responsável por chamar `IUnitOfWork.SaveChangesAsync()` ao final da operação. `ApplicationDbContext` implementa `IUnitOfWork`.

Repositórios específicos estendem `BaseRepository<T>`:
- `UsersRepository` — `GetByEmailAsync`, `GetByRefreshTokenAsync`, `EmailExistsAsync`
- `CourtsRepository` — override `GetAllAsync`/`GetByIdAsync` com `.Include(Sports)` e `.AsSplitQuery()`
- `SportsRepository` — `ExistsByNameAsync` (ILike), `GetByNameAsync`, `GetSportsByIdsAsync`
- `ReservationRepository` — `GetByCourtAndDayAsync`, `ExistsConflictAsync`
- `TenantRepository` — standalone (usa `TenantDbContext`), `GetBySlugAsync`, `SlugExistsAsync`

### 7.2. Caching (Redis)

`CacheService` implementa `ICacheService` usando `IDistributedCache` (StackExchange.Redis):
- Serialização: `System.Text.Json` (camelCase)
- TTL padrão: 30 minutos
- Prefixos de chave via enum `CacheKeyPrefix`: `UserById`, `CourtAvailability`, `TenantBySlug`, etc.

> `CacheService` está corretamente no projeto `Infrastructure` com namespace `Infrastructure.Services`. A interface `ICacheService` fica em `Application.Common.Interfaces`.

**Redis configurado via** `RedisExtensions.AddRedis()` com `RedisOptions` (validação por DataAnnotations).

### 7.3. Seeders

| Seeder | Schema | Momento | Descrição |
|---|---|---|---|
| `SuperAdminSeeder` | `public` | Startup (`Program.cs`) | Cria SuperAdmin se não existir |
| `SportSeeder` | tenant | Provisioning | Seeds de esportes padrão (6 esportes) |
| `CustomUserSeeder` | tenant | Provisioning | Seed de usuários customizados |

### 7.4. EF Core Configurations (Fluent API)

- `TenantConfiguration` — tabela fixa em `"public".Tenants`, index único em Slug e CustomDomain
- `UserConfiguration` — index único em Email, Role como string, `FullName` ignorado
- `CourtConfiguration` — `PricePerHour` com precision(10,2), index em Name
- `ReservationConfiguration` / `SportConfiguration` — configurações básicas

### 7.5. Schema Dinâmico via HasDefaultSchema

O isolamento de dados por tenant é feito via `HasDefaultSchema` no `OnModelCreating` do `ApplicationDbContext`:
- `ITenantContext.Schema` retorna `tenant_{slug}` (convertendo hífens em underscores)
- O EF Core qualifica todas as tabelas com o schema: `SELECT * FROM "tenant_arena1"."Users"`
- `TenantModelCacheKeyFactory` gera chave de cache por schema para que modelos de schemas diferentes não compartilhem cache
- **Não usa interceptor `SET search_path`** — o schema é nativo no modelo EF Core

### 7.6. ApplicationDbContextFactory

Factory para criar `ApplicationDbContext` fora do pipeline HTTP (migrations, provisioning, seeds):
- `Create(schema)` — cria contexto para um schema específico
- `CreateForTenant(tenant)` — cria contexto para o schema do tenant
- `CreateForPublic()` — cria contexto para o schema `public`
- Usa classes internas `FactoryTenantContext` e `FactoryCurrentUserService` para satisfazer dependências

---

## 8. Camada Api

### 8.1. Endpoints

| Grupo | Rota Base | Auth | Usa MediatR |
|---|---|---|---|
| `AuthEndpoints` | `/auth` | Anônimo | ✅ |
| `SportsEndpoints` | `/api/sports` | Misto (GET público) | ✅ |
| `CourtsEndpoints` | `/api/courts` | Misto (GET/availability público) | ✅ |
| `AdminStatsEndpoints` | `/admin/stats` | RequireAuth | ✅ |
| `TenantEndpoints` | `/api/tenants` | SuperAdmin | ✅ |
| Branding (`/api/branding`) | — | Anônimo | ❌ (ITenantContext direto) |
| Settings (`/api/settings`) | — | RequireAuth | ✅ |

### 8.2. Registro de Endpoints

Todos registrados em `AppExtensions.UseEndpoints()`:
```csharp
app.MapAuthEndpoints();
app.MapSportsEndpoints();
app.MapCourtsEndpoints();
app.MapAdminStatsEndpoints();
app.MapTenantEndpoints();
```

### 8.3. Middleware Pipeline

```
Request
  → UseRouting
  → UseCors
  → TenantResolutionMiddleware  (resolve tenant, preenche ITenantContext)
  → UseAuthentication           (valida JWT)
  → UseAuthorization            (verifica policies)
  → Endpoint Handler
```

---

## 9. Serviços de Aplicação

| Serviço | Interface | Descrição |
|---|---|---|
| `JwtService` | `IJwtService` | Gera access tokens (2h) e refresh tokens (7d) |
| `CacheService` | `ICacheService` | Wrapper Redis com serialização JSON |
| `ReservationService` | `IReservationService` | Lógica de disponibilidade e reserva de slots |
| `CurrentUserService` | `ICurrentUserService` | Extrai `UserId` do JWT (ClaimTypes.NameIdentifier) |
| `UserService` | `IUserService` | CRUD de roles e busca de users |
| `TenantContext` | `ITenantContext` | Estado Scoped do tenant resolvido |
| `TenantProvisioningService` | `ITenantProvisioningService` | Provisiona schema + migrations + seed |
| `TenantUsersQueryService` | `ITenantUsersQueryService` | Consulta users de um tenant específico (cross-schema) |
| `PasswordService` | `IPasswordService` | Hash/verify com PBKDF2-SHA256 |

---

## 10. Integrações Externas

| Sistema | Objetivo | Protocolo |
|---|---|---|
| **PostgreSQL 16** | Banco de dados principal (multi-schema) | TCP/5432 |
| **Redis 7** | Cache distribuído (tenant resolution, dados) | TCP/6379 |
| **pgAdmin 4** | Administração do banco (dev only) | HTTP/5050 |
| **Redis Commander** | Visualização do cache (dev only) | HTTP/8081 |

> Não há integrações com serviços externos (pagamento, email, SMS, etc.) no momento.

---

## 11. Pontos Críticos ("Gotchas")

### ⚠️ Obrigatório conhecer antes de contribuir:

1. **`BadRequest` retorna 400** — O error type `BadRequest` em `Common/Errors/BadRequest.cs` tem metadata `StatusCode: 400`. O HTTP 422 é reservado para erros de validação do FluentValidation (via `CustomExceptionHandler`). O HTTP 400 real para `DbUpdateException` com `UniqueViolation` continua no `CustomExceptionHandler`.

2. **`Tenant` não implementa `IEntity` nem `AuditEntity`** — `Tenant` é standalone (tem `Id` e `CreatedAt` próprios). O `TenantRepository` é completamente separado, opera via `TenantDbContext` no schema `public`, e não herda `BaseRepository<T>`.

3. **Soft Delete automático** — `ApplicationDbContext.SaveChangesAsync` intercepta `EntityState.Deleted` e converte em `MarkAsDeleted(userId)`. **Nunca faz DELETE real** em entidades que herdam `AuditEntity`. Global Query Filter aplica `WHERE "IsDeleted" = false` automaticamente.

4. **Schema dinâmico via `HasDefaultSchema`** — O schema do tenant é definido em `OnModelCreating` via `builder.HasDefaultSchema(schema)`. `TenantModelCacheKeyFactory` gera chave de cache por schema. **Não usa interceptor `SET search_path`**.

5. **Unit of Work explícito** — `BaseRepository` **não** chama `SaveChangesAsync`. O handler deve chamar `IUnitOfWork.SaveChangesAsync()` (implementado por `ApplicationDbContext`). Exceção: `ReservationService` chama `_unitOfWork.SaveChangesAsync()` internamente.

6. **CORS permite qualquer subdomínio de localhost** — Em dev, `SetIsOriginAllowed` retorna true para qualquer `*.localhost`. Em produção, depende da lista configurada em `Cors:AllowedOrigins`.

7. **SuperAdmin seed acontece no startup** — `SuperAdminSeeder` executa em `Program.cs` antes de qualquer request. Usa `ApplicationDbContextFactory.CreateForPublic()` para criar o contexto no schema `public`.

8. **`UserRole.SuperAdmin = 99`** — Valor alto intencional para separação clara dos roles normais (0, 1, 2).

9. **Provisioning cria Owner User** — `TenantProvisioningService.ProvisionAsync` cria automaticamente um user Admin com senha `Owner@123` no schema do tenant se `OwnerEmail` for fornecido.

10. **Migrations em startup (dev)** — `ExecuteMigrations()` migra schema `public` (TenantDbContext + ApplicationDbContext) e **todos** os schemas de tenant existentes. Pode ser lento com muitos tenants.

11. **Branding e Settings são endpoints separados** — `/api/branding` (público, sem auth) está em `BrandingEndpoints.cs` e `/api/settings` (autenticado, tenant self-service) está em `SettingsEndpoints.cs`. Ambos são tenant-scoped (acessados via subdomínio) e **não** pertencem ao grupo SuperAdmin de `TenantEndpoints`.

---

## 12. Mapa de Navegação

| O que procuro | Onde encontro |
|---|---|
| **Entidades de domínio** | `src/SportHub.Domain/entities/` |
| **Enums** | `src/SportHub.Domain/Enums/` |
| **Interfaces (contratos)** | `src/SportHub.Application/Common/Interfaces/` |
| **Erros tipados** | `src/SportHub.Application/Common/Errors/` |
| **CQRS interfaces** | `src/SportHub.Application/CQRS/` |
| **UseCases (Commands/Queries/Handlers)** | `src/SportHub.Application/UseCases/{Domínio}/{Ação}/` |
| **Validators** | Junto ao Command/Query no UseCase correspondente |
| **Services de aplicação** | `src/SportHub.Application/Services/` |
| **Settings/Config** | `src/SportHub.Application/Settings/` |
| **Authorization Policies** | `src/SportHub.Application/Security/` |
| **Repositórios** | `src/SportHub.Infrastructure/Repositories/` |
| **DbContexts** | `src/SportHub.Infrastructure/Persistence/` |
| **EF Configurations** | `src/SportHub.Infrastructure/Persistence/Configurations/` |
| **Migrations** | `src/SportHub.Infrastructure/Persistence/Migrations/` |
| **DbContext Factory** | `src/SportHub.Infrastructure/Persistence/ApplicationDbContextFactory.cs` |
| **Security (Password, Auth Handlers)** | `src/SportHub.Infrastructure/Security/` |
| **Services de infra (Seeders, Tenant)** | `src/SportHub.Infrastructure/Services/` |
| **Tenant Middleware** | `src/SportHub.Infrastructure/Middleware/TenantResolutionMiddleware.cs` |
| **Endpoints (Minimal APIs)** | `src/SportHub.Api/Endpoints/` |
| **DI / Service Registration** | `src/SportHub.Api/Extensions/ServiceExtensions.cs` |
| **Endpoint Registration** | `src/SportHub.Api/Extensions/AppExtensions.cs` |
| **Exception Handler** | `src/SportHub.Api/Middleware/CustomExceptionHandler.cs` |
| **MediatR Behaviors** | `src/SportHub.Api/Behaviors/` + `src/SportHub.Application/Behaviors/` |
| **Result → IResult conversion** | `src/SportHub.Api/Extensions/ResultExtensions/ResultExtensions.cs` |
| **Configuração (appsettings)** | `src/SportHub.Api/appsettings.*.json` |
| **Docker** | `docker-compose.yml` |
| **Testes** | `tests/SportHub.Tests/` |

---

## 13. Guia de Padronização (Style Guide)

### 13.1. Criando um novo UseCase

1. Crie pasta `UseCases/{Domínio}/{Ação}/`
2. Crie o Command/Query como `record` implementando `ICommand<TResponse>` ou `IQuery<TResponse>`
3. Crie o Handler implementando `ICommandHandler<TCmd, TResp>` ou `IQueryHandler<TQuery, TResp>`
4. Crie o Validator como `AbstractValidator<TCommand>`
5. Retorne `Result.Ok(...)` para sucesso ou `Result.Fail(new ErrorType("msg"))` para erro
6. **Nunca** lance exceções para erros de negócio — use o Result Pattern

### 13.2. Criando um novo Endpoint

1. Crie `{Entidade}Endpoints.cs` em `Api/Endpoints/`
2. Use `static class` com extension method `Map{Entidade}Endpoints`
3. Use `ISender` (MediatR) para despachar commands/queries
4. Converta resultado com `.ToIResult()` ou `.ToIResult(StatusCodes.Status201Created)`
5. Registre em `AppExtensions.UseEndpoints()`
6. Documente com `.WithName()`, `.WithSummary()`, `.Produces<T>()`

### 13.3. Criando uma nova Entidade

1. Crie em `Domain/entities/`
2. Herde de `AuditEntity` e implemente `IEntity`
3. Crie `IEntityTypeConfiguration<T>` em `Infrastructure/Persistence/Configurations/`
4. Adicione `DbSet<T>` no `ApplicationDbContext`
5. Crie `I{Entidade}Repository` em `Application/Common/Interfaces/`
6. Implemente `{Entidade}Repository : BaseRepository<T>` em `Infrastructure/Repositories/`
7. Registre no DI em `ServiceExtensions.AddRepositories()`

### 13.4. Regras de Ouro

- **Result Pattern sempre**: handlers retornam `Result<T>`, nunca lançam exceções para erros de negócio
- **FluentValidation**: toda validação de input via Validator + `ValidationBehavior` pipeline
- **Interfaces no Application**: todas as abstrações (`I*`) ficam em `Application/Common/Interfaces/`
- **Implementações no Infrastructure**: repositórios, services concretos, security handlers
- **DI manual**: todo registro é explícito em `ServiceExtensions` (não há auto-scan)
- **Scoped por padrão**: services e repos são Scoped (exceto `ValidationBehavior` que é Transient)
- **Tenant-aware**: todo código que acessa dados de tenant deve usar `ApplicationDbContext` (que já tem schema correto via `HasDefaultSchema`)
- **Unit of Work**: handlers devem chamar `IUnitOfWork.SaveChangesAsync()` ao final da operação — repositórios não fazem SaveChanges
