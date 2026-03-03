# SportHub — Especificação Técnica e Diretrizes de Arquitetura

> **Versão:** 2.0 | **Data:** 2026-03-03 | **Stack:** .NET 9 (C#) — Backend Monolito

---

## 1. Visão Geral

**SportHub** é uma API REST para gestão de estabelecimentos esportivos, quadras e reservas. Permite que donos de estabelecimentos cadastrem seus espaços, configurem quadras com horários e slots, e que usuários comuns realizem reservas de quadras por horário. É o backend central consumido pelo frontend SPA em repositório separado (`sporthub-front-end`).

---

## 2. Stack Tecnológico

### 2.1 Linguagens e Frameworks

| Camada | Tecnologia | Versão |
|--------|-----------|--------|
| Runtime | .NET | 9.0 |
| Linguagem | C# | 13 (via net9.0) |
| Web Framework | ASP.NET Core Minimal API | 9.0 |
| ORM | Entity Framework Core + Npgsql | 9.0.7 |
| Mediator/CQRS | MediatR | 13.0.0 |
| Validação (Application) | FluentValidation | 12.0.0 |
| Validação (Api) | FluentValidation.AspNetCore | 11.3.1 |
| Erros tipados | FluentResults | 4.0.0 |
| Logging | Serilog (Console + File) | 9.0.0 |
| Cache | StackExchange.Redis (IDistributedCache) | 9.0.7 |
| Auth | JWT Bearer (HMAC-SHA256) + Policies ASP.NET | 9.0.7 |
| Documentação API | Microsoft.AspNetCore.OpenApi + Scalar | 9.0.7 / 2.6.1 |
| Segurança de senha | System.IdentityModel.Tokens.Jwt | 8.13.0 |

### 2.2 Infraestrutura (Docker Compose)

| Serviço | Imagem | Porta | Uso |
|---------|--------|-------|-----|
| API | Dockerfile local | 5001→8080 | Backend principal |
| Banco de dados | postgres:16 | 5432 | Persistência relacional |
| Cache | redis:7 | 6379 | Cache distribuído |
| Admin BD | dpage/pgadmin4 | 5050 | UI admin PostgreSQL |
| Admin Cache | redis-commander | 8081 | UI admin Redis |

### 2.3 Ferramentas de Build

- **MSBuild** via `dotnet CLI` / `.sln` (SportHub.sln)
- Nullable references habilitado globalmente (`<Nullable>enable</Nullable>`)
- Implicit usings habilitado em todos os projetos
- Global usings em Application: `MediatR` e `FluentResults`

---

## 3. Estrutura do Repositório

```
SportHub/
├── src/
│   ├── SportHub.Api/            # Presentation Layer — Minimal API, Endpoints, Middlewares
│   ├── SportHub.Application/    # Application Layer — CQRS, Use Cases, Interfaces, Services
│   ├── SportHub.Domain/         # Domain Layer — Entities, Value Objects, Enums
│   └── SportHub.Infrastructure/ # Infrastructure Layer — EF Core, Repos, Security, Seeders
├── tests/
│   └── SportHub.Tests/          # Projeto de testes (estrutura inicial)
├── documentos/                  # Documentação técnica
├── docker-compose.yml
└── SportHub.sln
```

### 3.1 Dependências entre Projetos

```
SportHub.Api ─────────┬── SportHub.Application ──── SportHub.Domain
                      └── SportHub.Infrastructure ─┬── SportHub.Application
                                                   └── SportHub.Domain
```

> **Regra de ouro:** Domain não depende de nenhuma camada. Application depende apenas do Domain. Infrastructure implementa contratos do Application. Api orquestra o bootstrap e DI.

---

## 4. Arquitetura — Clean Architecture (4 Camadas)

### 4.1 Domain Layer (`SportHub.Domain`)

Responsável por: entidades de negócio, value objects, enums. **Zero dependências externas.**

**Entidades:**

| Entidade | Herda de | Implementa | Descrição |
|----------|----------|------------|-----------|
| `User` | `AuditEntity` | `IEntity` | Usuário da plataforma com role global |
| `Establishment` | `AuditEntity` | `IEntity` | Estabelecimento esportivo com endereço |
| `EstablishmentUser` | `AuditEntity` | — | Tabela de junção: vínculo usuário↔estabelecimento com role |
| `Court` | `AuditEntity` | `IEntity` | Quadra com configuração de slots e horários |
| `Sport` | `AuditEntity` | `IEntity` | Esporte (dados iniciais via seeder) |
| `Reservation` | `AuditEntity` | `IEntity` | Reserva de quadra com horário UTC |

**IEntity:** interface marcadora com `Guid Id { get; }` — obrigatória para usar `BaseRepository<T>`.

**AuditEntity (classe base abstrata):**

Toda entidade persistida herda `AuditEntity`, que gerencia automaticamente:
- `CreatedAt`, `CreatedBy` — setados via `SetCreated(userId)`
- `UpdatedAt`, `UpdatedBy` — setados via `SetUpdated(userId)`
- `IsDeleted`, `DeletedAt`, `DeletedBy` — soft delete via `MarkAsDeleted(userId)` / `Restore()`
- **Importante:** `EstablishmentUser` herda `AuditEntity` mas NÃO implementa `IEntity` (não possui `Id` próprio, usa chave composta)

**Value Objects:**

| VO | Tipo EF | Propriedades |
|----|---------|-------------|
| `Address` | Owned Entity | `Street`, `Number`, `Complement?`, `Neighborhood`, `City`, `State`, `ZipCode` |

**Enums:**

| Enum | Valores | Uso |
|------|---------|-----|
| `UserRole` | `Admin`, `EstablishmentMember`, `User` | Role global do usuário (tabela `Users`) |
| `EstablishmentRole` | `Staff=0`, `Manager=1`, `Owner=2` | Role por estabelecimento (tabela `EstablishmentUsers`) |

**Relações principais:**
- `User` ↔ `Establishment`: N:N via `EstablishmentUser` (com role)
- `Establishment` → `Court`: 1:N
- `Establishment` ↔ `Sport`: N:N (EF many-to-many)
- `Court` ↔ `Sport`: N:N
- `Court` → `Reservation`: 1:N
- `User` → `Reservation`: 1:N

### 4.2 Application Layer (`SportHub.Application`)

Responsável por: casos de uso (CQRS), interfaces de contratos, validadores, behaviors, serviços de domínio.

**Global Usings:** `MediatR` e `FluentResults` (importados globalmente para toda a camada).

#### 4.2.1 CQRS via MediatR

```csharp
// Commands (escrita)
ICommand              → IRequest<Result>
ICommand<TResponse>   → IRequest<Result<TResponse>>
ICommandHandler<TCommand>            → IRequestHandler<TCommand, Result>
ICommandHandler<TCommand, TResponse> → IRequestHandler<TCommand, Result<TResponse>>

// Queries (leitura)
IQuery<TResponse>                    → IRequest<Result<TResponse>>
IQueryHandler<TQuery, TResponse>     → IRequestHandler<TQuery, Result<TResponse>>
```

#### 4.2.2 Organização dos Use Cases

```
UseCases/
├── Auth/
│   ├── AuthResponse.cs                  (response compartilhado Login/Register)
│   ├── Login/                           (LoginCommand, LoginHandler, LoginValidator)
│   ├── RegisterUser/                    (RegisterUserCommand, RegisterUserHandler, RegisterUserValidator)
│   └── DeleteUser/                      (DeleteUserCommand, DeleteUserHandler)
├── Establishments/
│   ├── CreateEstablishment/             (Command, Handler, Validator)
│   ├── UpdateEstablishment/             (Command, Handler, Validator)
│   ├── DeleteEstablishment/             (Command, Handler)
│   ├── ActiveEstablishment/             (Command, Handler)
│   ├── GetEstablishments/               (Query, Handler, Response) ← paginação
│   └── GetEstablishmentById/            (Query, Handler, Response)
├── EstablishmentUser/
│   └── CreateEstablishmentUser/         (Command, Handler, Validator, Request/Response)
├── Court/
│   ├── CreateCourt/                     (Command, Handler, Validator)
│   ├── GetAvailability/                 (Query, Handler, Response) ← usa Redis cache
│   └── GetCourtsByEstablishmentId/      (Query, Handler, Response)
└── Reservation/
    └── CreateReservation/               (Command, Handler, Validator, Request/Response)
```

#### 4.2.3 Pipeline Behaviors (MediatR)

| Behavior | Camada | Scope DI | Responsabilidade |
|----------|--------|----------|-----------------|
| `LoggingBehavior<,>` | Api | Scoped | Log de entrada/saída com `Stopwatch` e tempo de execução |
| `ValidationBehavior<,>` | Application | Transient | Executa todos `IValidator<TRequest>` em paralelo; lança `ValidationException` se falhar |

Ordem de execução: Logging → Validation → Handler.

#### 4.2.4 Erros Tipados (FluentResults)

Todos os handlers retornam `Result<T>` ou `Result`. Erros modelados como subclasses de `Error` com metadado `StatusCode`:

| Classe de Erro | StatusCode HTTP | Uso |
|---------------|----------------|-----|
| `NotFound` | 404 | Recurso não encontrado |
| `BadRequest` | **422** | Dados inválidos de negócio |
| `Conflict` | 409 | Conflito (ex: email duplicado) |
| `Forbidden` | 403 | Sem permissão |
| `Unauthorized` | 401 | Não autenticado |
| `InternalServerError` | 500 | Erro interno |

> **Atenção:** `BadRequest` retorna **422** (não 400). Erros genéricos `Result.Fail("msg")` sem tipo específico resultam em status 400 no `ResultExtensions`.

#### 4.2.5 Interfaces de Contratos (`Common/Interfaces/`)

**Repositórios:**

| Interface | Descrição |
|-----------|-----------|
| `IBaseRepository<T>` | CRUD genérico (`GetById`, `GetAll`, `Add`, `Update`, `Remove`, `GetByIds`, `Exists`, `Query`, `AddMany`) |
| `IEstablishmentsRepository` | + `GetByIdsWithDetailsAsync`, `GetByIdWithAddressAsync`, `GetFilteredAsync` |
| `IEstablishmentUsersRepository` | + `GetAsync`, `GetByOwnerIdAsync`, `HasRoleAnywhereAsync` |
| `IUsersRepository` | + `GetByEmailAsync`, `EmailExistsAsync` |
| `ICourtsRepository` | Queries específicas de quadra |
| `ISportsRepository` | + `GetSportsByIdsAsync`, `ExistsByNameAsync` |
| `IReservationRepository` | + `GetByCourtAndDayAsync`, `ExistsConflictAsync` |

**Serviços:**

| Interface | Responsabilidade |
|-----------|-----------------|
| `IJwtService` | Geração de token JWT |
| `IPasswordService` | Hash/verificação de senha (PBKDF2) |
| `ICurrentUserService` | `UserId` do usuário autenticado via `HttpContext` |
| `IUserService` | CRUD de usuário, gestão de roles |
| `IEstablishmentService` | Operações de negócio sobre estabelecimentos |
| `IEstablishmentRoleService` | Verificação de roles por estabelecimento |
| `IReservationService` | Disponibilidade de slots e criação de reserva |
| `ICacheService` | Abstração de cache Redis (`Get`, `Set`, `Remove`, `Exists`, `GenerateCacheKey`) |

#### 4.2.6 Serviços de Aplicação

| Serviço | Camada Real | Responsabilidade |
|---------|------------|-----------------|
| `JwtService` | Application | Gera JWT com claims; expiração hardcoded em **2h** (ignora `ExpiryMinutes` do appsettings) |
| `CacheService` | Application* | Wrapper sobre `IDistributedCache` com serialização JSON; TTL padrão 30min |
| `ReservationService` | Application | Geração de slots disponíveis + validação e criação de reserva |
| `EstablishmentRoleService` | Application | Verificação hierárquica de roles (`HasAtLeastRoleAsync`) |
| `EstablishmentService` | Application | Busca de estabelecimentos com detalhes |

> \* `CacheService` está em `Application/Services/` mas usa namespace `Infrastructure.Services` — inconsistência de namespace.

#### 4.2.7 Cache

Chaves geradas por `CacheKeyPrefix` enum + argumentos variádicos: `{Prefix}:{arg1}:{arg2}`.

Prefixos disponíveis (`CacheKeyPrefix`): `UserById`, `CourtAvailability`, `ReservationList`, `EstablishmentSummary`, `GetAvailability`.

Padrão de uso:
```csharp
var key = _cache.GenerateCacheKey(CacheKeyPrefix.GetAvailability, courtId, date);
var cached = await _cache.GetAsync<TResponse>(key, ct);
if (cached != null) return Result.Ok(cached);
// ... calcular ...
await _cache.SetAsync(key, response, TimeSpan.FromMinutes(30), ct);
```

#### 4.2.8 Validação (FluentValidation)

- Um validador por Command, no mesmo diretório do Use Case
- Herdando `AbstractValidator<TCommand>`
- Registrados automaticamente via `AddValidatorsFromAssemblyContaining<RegisterUserValidator>()`
- Executados pelo `ValidationBehavior` no pipeline MediatR — **nunca chamar validators manualmente**
- Extensão customizada: `PasswordValidatorExtensions.Password()` — mínimo 8 chars, 1 maiúscula, 1 dígito, 1 especial

### 4.3 Infrastructure Layer (`SportHub.Infrastructure`)

Responsável por: persistência (EF Core), implementação de repositórios, serviços de segurança, seeders.

#### 4.3.1 ApplicationDbContext

- Herda `DbContext`, recebe `ICurrentUserService` via construtor
- DbSets: `Users`, `Establishments`, `EstablishmentUsers`, `Courts`, `Sports`, `Reservations`
- `OnModelCreating`: carrega configurações via `ApplyConfigurationsFromAssembly`
- **`SaveChangesAsync` sobrescrito** para audit automático:
  - `Added` → `SetCreated(userId)`
  - `Modified` → `SetUpdated(userId)`
  - `Deleted` → converte para `Modified` + `MarkAsDeleted(userId)` (soft delete automático)
  - Owned entities têm estado preservado como `Unchanged` em deleções

#### 4.3.2 Repositórios

`BaseRepository<T>` implementa `IBaseRepository<T>` com constraint `where T : class, IEntity`:

| Método | Comportamento |
|--------|--------------|
| `AddAsync` | `_dbSet.AddAsync` + `SaveChangesAsync` (commit imediato) |
| `UpdateAsync` | `_dbSet.Update` + `SaveChangesAsync` |
| `RemoveAsync` | `_dbSet.Remove` + `SaveChangesAsync` (soft delete via DbContext) |
| `Query()` | Retorna `IQueryable<T>` para queries customizadas |

Repositórios específicos herdam `BaseRepository<T>`:

| Repositório | Métodos adicionais chave |
|-------------|------------------------|
| `EstablishmentsRepository` | `GetByIdsWithDetailsAsync` (com Include+SplitQuery+NoTracking), `GetFilteredAsync` (paginação com projeção) |
| `EstablishmentUsersRepository` | `GetAsync(userId, estId)`, `GetByOwnerIdAsync`, `HasRoleAnywhereAsync` |
| `ReservationRepository` | `GetByCourtAndDayAsync`, `ExistsConflictAsync` (overlap temporal) |
| `UsersRepository` | `GetByEmailAsync`, `EmailExistsAsync` |
| `SportsRepository` | `GetSportsByIdsAsync`, `ExistsByNameAsync` |

#### 4.3.3 Configurações EF Core

Uma `IEntityTypeConfiguration<T>` por entidade em `Persistence/Configurations/`:

| Configuração | Destaques |
|-------------|-----------|
| `UserConfiguration` | Índice único em `Email`, índice em `Role`, `Role` convertido para string |
| `EstablishmentConfiguration` | Índice em `Name`, `Address` como Owned Entity (índices em `City`, `State`) |
| `EstablishmentUserConfiguration` | Chave composta `(EstablishmentId, UserId)` |
| `EstablishmentSportConfiguration` | Configuração da relação N:N entre Establishment e Sport |
| `CourtConfiguration` | Configuração de propriedades de quadra |
| `ReservationConfiguration` | FK para Court e User |
| `SportConfiguration` | Configuração básica de Sport |

#### 4.3.4 Seeders (executados no startup)

| Seeder | Comportamento |
|--------|--------------|
| `CustomUserSeeder` | Cria usuário Admin se não existir (email de `AdminUserSettings`). Role `Admin`, ativo |
| `SportSeeder` | Popula 8 esportes base: Football, Basketball, Volleyball, Tennis, Futsal, Handball, Beach Tennis, Squash. Verifica por nome antes de inserir |

#### 4.3.5 Segurança

| Serviço | Implementação |
|---------|--------------|
| `PasswordService` | PBKDF2/SHA-256, 10.000 iterações, salt 32 bytes, hash 64 bytes |
| `EstablishmentHandler` | `AuthorizationHandler<EstablishmentRequirement, HttpContext>` — extrai `establishmentId` da rota, verifica role hierárquico; **Admin e User têm bypass** |
| `GlobalRoleHandler` | Verifica se usuário tem determinado `EstablishmentRole` em qualquer estabelecimento |
| `CurrentUserService` | Extrai `UserId` de `ClaimTypes.NameIdentifier` via `IHttpContextAccessor` |

### 4.4 API Layer (`SportHub.Api`)

Responsável por: exposição HTTP, roteamento, middlewares, DI bootstrap, logging pipeline.

**Estilo:** Minimal API com `MapGroup` e extensões estáticas por domínio.

#### 4.4.1 Endpoints

| Grupo | Prefixo | Arquivo | Rotas principais |
|-------|---------|---------|-----------------|
| Auth | `/auth` | `AuthEndpoints.cs` | `POST /register`, `POST /login` |
| Establishments | `/establishments` | `EstablishmentsEndpoints.cs` | CRUD + `/activate` + `/{id}/users` + `/{id}/courts` |
| Courts | `/courts` | `CourtsEndpoints.cs` | `GET /{id}/availability/{date}`, `POST /{id}/reservations` |
| Sports | `/api/sports` | `SportsEndpoints.cs` | `GET /` (lista todos) |

Endpoints registrados em `AppExtensions.UseEndpoints()`.

**Padrão de endpoint:**

```csharp
group.MapPost("/", async (CreateXCommand command, ISender sender) => {
    var result = await sender.Send(command);
    return result.ToIResult(StatusCodes.Status201Created);
})
.WithName("CreateX").WithSummary("...").WithDescription("...")
.Produces<XResponse>(201)
.Produces<ProblemDetails>(400)
.RequireAuthorization(PolicyNames.IsEstablishmentManager);
```

#### 4.4.2 ResultExtensions (`ToIResult`)

Converte `Result<T>` / `Result` em `IResult` HTTP:
- Sucesso com valor → `Results.Json(value, statusCode)` (default 200)
- Sucesso sem valor → `Results.NoContent()`
- Falha → `Results.Problem(...)` com `ProblemDetails` contendo:
  - `StatusCode` extraído do metadata do primeiro erro (fallback 400)
  - `traceId` do `HttpContext`
  - Lista de erros com mensagens e metadados

#### 4.4.3 Exception Handlers

**`CustomExceptionHandler` (IExceptionHandler):**

| Exceção | Status | Título |
|---------|--------|--------|
| `ValidationException` (FluentValidation) | 422 | Validation Failed |
| `DbUpdateException` (PostgreSQL unique constraint) | 400 | Unique Constraint Violation |
| Qualquer outra | 500 | Internal Server Error |

**`CustomAuthorizationMiddlewareResultHandler` (IAuthorizationMiddlewareResultHandler):**

| Situação | Status | Mensagem |
|----------|--------|----------|
| Challenged (token ausente/inválido) | 401 | "Token faltando ou inválido" |
| Forbidden (sem permissão) | 403 | "You don't have permission to access this resource" |

Ambos retornam `ProblemDetails` com `traceId`.

#### 4.4.4 Bootstrap / DI (ServiceExtensions)

Todos os registros de DI são feitos via extension methods no `WebApplicationBuilder`:

| Método | Responsabilidade |
|--------|-----------------|
| `AddAuthentication()` | JWT Bearer config + 6 Authorization Policies |
| `AddServices()` | Registro de todos os serviços (Scoped) |
| `AddRepositories()` | Registro de todos os repositórios (Scoped) |
| `AddCustomExceptionHandler()` | Exception handler + Authorization middleware + ProblemDetails |
| `AddDatabase()` | EF Core + Npgsql com connection string do appsettings |
| `AddMediatR()` | MediatR + FluentValidation + Pipeline Behaviors |
| `AddSettings()` | `JwtSettings` + `AdminUserSettings` via `IOptions<T>` |
| `AddSeeders()` | `CustomUserSeeder` + `SportSeeder` (Transient) |
| `AddSerilogLogging()` | Serilog com config do appsettings |
| `AddCaching()` | Redis via `RedisExtensions.AddRedis()` |

---

## 5. Segurança e Autorização

### 5.1 Autenticação JWT

- **Algoritmo:** HMAC-SHA256
- **Claims:** `NameIdentifier` (userId), `Email`, `Name` (fullName), `Role` (UserRole)
- **Expiração:** 2h (hardcoded no `JwtService`)
- **Validação:** issuer, audience, lifetime, signing key
- **Config:** seção `Jwt` no appsettings (`Key`, `Issuer`, `Audience`, `ExpiryMinutes`)
- **Gotcha:** `ExpiryMinutes` do appsettings **não é usado** — expiração está hardcoded como `AddHours(2)` no `JwtService`

### 5.2 Modelo de Papéis Dual

**Global (UserRole):** papel fixo na tabela `Users`, gravado como claim no JWT.

| Role | Acesso |
|------|--------|
| `Admin` | Acesso total, bypass de todas verificações de estabelecimento |
| `EstablishmentMember` | É dono/gerente/staff de pelo menos um estabelecimento |
| `User` | Usuário padrão, pode fazer reservas |

**Por Estabelecimento (EstablishmentRole):** armazenado em `EstablishmentUsers`, verificado em runtime.

| Role | Nível | Hierarquia |
|------|-------|-----------|
| `Staff` | 0 | Acesso básico |
| `Manager` | 1 | ≥ Staff |
| `Owner` | 2 | ≥ Manager ≥ Staff |

Verificação hierárquica: `HasAtLeastRoleAsync` compara `actual >= required`.

### 5.3 Authorization Policies

| Policy | Tipo | Verificação |
|--------|------|------------|
| `IsStaff` | Global | Tem `Staff` em qualquer estabelecimento |
| `IsManager` | Global | Tem `Manager` em qualquer estabelecimento |
| `IsOwner` | Global | Tem `Owner` em qualquer estabelecimento |
| `IsEstablishmentStaff` | Por Estabelecimento | `Staff+` no `{establishmentId}` da rota |
| `IsEstablishmentManager` | Por Estabelecimento | `Manager+` no `{establishmentId}` da rota |
| `IsEstablishmentOwner` | Por Estabelecimento | `Owner` no `{establishmentId}` da rota |

### 5.4 Senha

- **Algoritmo:** PBKDF2 com SHA-256
- **Iterações:** 10.000
- **Salt:** 32 bytes aleatórios (Base64)
- **Hash:** 64 bytes (Base64)
- **Regras de validação:** mínimo 8 caracteres, 1 maiúscula, 1 dígito, 1 caractere especial (via `PasswordValidatorExtensions`)

---

## 6. Domínio de Negócio — Reservas

### 6.1 Configuração de Quadra (Court)

| Propriedade | Default | Descrição |
|-------------|---------|-----------|
| `SlotDurationMinutes` | 30 | Granularidade dos slots |
| `MinBookingSlots` | 1 | Mínimo de slots por reserva |
| `MaxBookingSlots` | 4 | Máximo de slots por reserva |
| `OpeningTime` | 08:00 | Abertura (`TimeOnly`) |
| `ClosingTime` | 22:00 | Fechamento (`TimeOnly`) |
| `TimeZone` | `America/Maceio` | Fuso horário da quadra |

### 6.2 Fluxo de Disponibilidade (com cache)

```
GET /courts/{courtId}/availability/{date}
    ↓
GetAvailabilityHandler
    ↓ check Redis → key: "GetAvailability:{courtId}:{date}" (TTL 30min)
    ↓ [miss]
    ↓
ReservationService.GetAvailableSlotsAsync(courtId, day)
    ├── busca Court no banco
    ├── gera slots: de OpeningTime até ClosingTime a cada SlotDuration
    ├── busca reservas do dia via GetByCourtAndDayAsync
    ├── filtra slots com conflito temporal (overlap)
    └── retorna List<DateTime> (slots disponíveis)
    ↓
[save no Redis] → return slots UTC
```

### 6.3 Fluxo de Criação de Reserva

```
POST /courts/{courtId}/reservations  [RequireAuthorization]
    ↓
CreateReservationHandler
    ↓
ReservationService.ReserveAsync(court, userId, startUtc, endUtc)
    ├── valida dentro do horário de funcionamento (Opening ≤ start, end ≤ Closing)
    ├── valida duração múltipla de SlotDuration
    ├── valida total de slots (min ≤ slots ≤ max)
    ├── verifica conflito no banco: ExistsConflictAsync (overlap temporal)
    └── persiste Reservation → retorna Guid do ID
```

---

## 7. Guia de Padronização (Style Guide)

### 7.1 Nomenclatura

| Artefato | Convenção | Exemplo |
|----------|-----------|---------|
| Entidade | PascalCase, singular | `Establishment`, `Court` |
| Command | `{Ação}{Entidade}Command` | `CreateEstablishmentCommand` |
| Query | `{Get}{Entidade}{Filtro}Query` | `GetCourtsByEstablishmentIdQuery` |
| Handler | `{UseCase}Handler` | `CreateEstablishmentHandler` |
| Validator | `{UseCase}Validator` | `RegisterUserValidator` |
| Response | `{UseCase}Response` | `GetEstablishmentsResponse` |
| Request (input extra) | `{Entidade}Request` | `CourtRequest`, `ReservationRequest` |
| Interface | `I{Nome}` | `IEstablishmentsRepository` |
| Repositório | `{Entidade}sRepository` | `EstablishmentsRepository` |
| Serviço | `{Nome}Service` | `ReservationService`, `JwtService` |
| Extensão estática | `{Nome}Extensions` | `ServiceExtensions`, `AppExtensions` |
| Endpoints | `{Entidade}Endpoints` | `EstablishmentsEndpoints` |
| Configuração EF | `{Entidade}Configuration` | `EstablishmentConfiguration` |
| Settings | `{Nome}Settings` | `JwtSettings`, `AdminUserSettings` |

### 7.2 Criação de Novo Use Case (passo a passo)

1. **Domain:** se necessário, adicionar/ajustar entidade em `SportHub.Domain/entities/` + enum/VO
2. **Application — Contrato:**
   - `{Feature}Command.cs` (record/class) implementando `ICommand<TResponse>` ou `ICommand`
   - Ou `{Feature}Query.cs` implementando `IQuery<TResponse>`
   - `{Feature}Response.cs` (se retorna dados)
3. **Application — Validação:**
   - `{Feature}Validator.cs` herdando `AbstractValidator<{Feature}Command>` (para Commands)
4. **Application — Handler:**
   - `{Feature}Handler.cs` implementando `ICommandHandler<,>` ou `IQueryHandler<,>`
   - Retorna `Result.Ok(...)` ou `Result.Fail(new {ErrorType}("..."))`
   - Injetar dependências via construtor (interfaces da Application)
5. **Infrastructure:** se necessário, adicionar método ao repositório existente ou criar novo
   - Interface em `Application/Common/Interfaces/`
   - Implementação em `Infrastructure/Repositories/`
   - Registrar DI em `ServiceExtensions.AddRepositories()`
6. **API:** mapear endpoint em `Endpoints/{Entidade}Endpoints.cs`
   - Usar `ISender` para enviar command/query
   - Converter resultado com `.ToIResult()` ou `.ToIResult(statusCode)`
   - Registrar grupo em `AppExtensions.UseEndpoints()`

### 7.3 Tratamento de Erros

**Na Application (handlers):** usar erros tipados FluentResults:
```csharp
return Result.Fail(new NotFound("Resource not found"));     // → 404
return Result.Fail(new BadRequest("Invalid data"));         // → 422
return Result.Fail(new Conflict("Already exists"));         // → 409
return Result.Fail(new Forbidden("No permission"));         // → 403
return Result.Fail(new Unauthorized("Not authenticated"));  // → 401
return Result.Ok(response);
```

**Na API:** sempre usar `.ToIResult()` — converte automaticamente para `ProblemDetails`.

**Regra:** **Nunca lançar exceções nos handlers.** Exceções são apenas para casos inesperados (tratadas pelo `CustomExceptionHandler`). Erros de negócio usam `Result.Fail`.

### 7.4 Banco de Dados / EF Core

- Toda entidade persistida herda `AuditEntity`
- **Soft delete automático:** `SaveChangesAsync` converte `EntityState.Deleted` para `Modified` + `MarkAsDeleted`. Nunca usar delete físico
- Criar `IEntityTypeConfiguration<T>` para cada entidade em `Infrastructure/Persistence/Configurations/`
- Usar `AsSplitQuery()` + `AsNoTracking()` em queries de leitura com múltiplos `Include`
- Adicionar índices explícitos em campos frequentemente filtrados
- `Address` é **owned entity** (coluna embutida, não tabela separada)
- Enums persistidos como **string** (ex: `UserRole` via `.HasConversion<string>()`)

### 7.5 Configuração / Settings

Configurações fortemente tipadas via `IOptions<T>`:

| Setting | Seção appsettings | Validação |
|---------|-------------------|-----------|
| `JwtSettings` | `Jwt` | Manual |
| `AdminUserSettings` | `AdminUser` | Manual |
| `RedisOptions` | `Redis` | `ValidateDataAnnotations` + `ValidateOnStart` |

---

## 8. Pontos Críticos ("Gotchas")

1. **JWT expiração hardcoded:** `JwtService` ignora `ExpiryMinutes` do appsettings e usa `AddHours(2)` fixo
2. **BadRequest = 422:** O erro tipado `BadRequest` retorna 422, não 400. Erros genéricos sem tipo retornam 400
3. **CacheService namespace:** `CacheService` está em `Application/Services/` mas com namespace `Infrastructure.Services`
4. **EstablishmentUser sem IEntity:** Não possui `Id` próprio — usa chave composta `(EstablishmentId, UserId)`. Não pode usar `BaseRepository<EstablishmentUser>`
5. **Soft delete global:** Todo `Remove` via DbContext faz soft delete. Para queries, lembrar de filtrar `IsDeleted == false` quando necessário
6. **Admin bypass:** `EstablishmentHandler` dá bypass automático para users com `UserRole.Admin` **e** `UserRole.User` — apenas `EstablishmentMember` é verificado no estabelecimento
7. **SportsEndpoints prefixo diferente:** Usa `/api/sports` enquanto os demais endpoints não têm prefixo `/api`
8. **Commit por operação:** `BaseRepository.AddAsync` faz `SaveChangesAsync` individualmente — operações compostas (ex: criar establishment + user) resultam em múltiplos saves

---

## 9. Integrações Externas

| Sistema | Protocolo | Uso |
|---------|-----------|-----|
| PostgreSQL 16 | EF Core / Npgsql | Persistência relacional principal |
| Redis 7 | IDistributedCache (StackExchange.Redis) | Cache distribuído (disponibilidade de quadras) |

O frontend (repositório `sporthub-front-end`) consome esta API via **REST/JSON** autenticado com **JWT Bearer** no header `Authorization`.

---

## 10. Documentação da API

- **OpenAPI** gerado automaticamente pelo ASP.NET Core 9
- **Scalar** como UI interativa de documentação (apenas em desenvolvimento)
- Acesso: `GET /scalar/v1` (development)
- Segurança documentada via `BearerSecuritySchemeTransformer`

---

## 11. Logging

- **Serilog** com configuração via appsettings (`ReadFrom.Configuration`)
- **Sinks:** Console + File
- **Enriquecimento:** `LogContext`
- **`LoggingBehavior`:** loga cada Command/Query com:
  - Nome do request
  - Payload completo (serializado)
  - Tempo de execução em ms
  - Status (sucesso ✅ / erro ❌)

---

## 12. Mapa de Navegação (Referências Rápidas)

| O que fazer | Onde olhar |
|------------|-----------|
| Criar novo use case | `src/SportHub.Application/UseCases/{Domínio}/{Feature}/` |
| Adicionar entidade | `src/SportHub.Domain/entities/` + `Infrastructure/Persistence/Configurations/` |
| Expor novo endpoint | `src/SportHub.Api/Endpoints/` + registrar em `AppExtensions.UseEndpoints()` |
| Novo repositório | Interface em `Application/Common/Interfaces/` + impl em `Infrastructure/Repositories/` + DI em `ServiceExtensions` |
| Novo serviço | Interface em `Application/Common/Interfaces/` + impl em `Application/Services/` ou `Infrastructure/Services/` + DI em `ServiceExtensions` |
| Nova policy de auth | `Application/Security/PolicyNames.cs` + `ServiceExtensions.AddAuthentication()` |
| Novo prefixo de cache | `Application/Common/Enums/CacheKeyPrefix.cs` |
| Novo tipo de erro | `Application/Common/Errors/` (herdar `Error` com metadado `StatusCode`) |
| Configuração de settings | `Application/Settings/` + seção em appsettings + `ServiceExtensions.AddSettings()` |
| Nova validação customizada | `Application/Extensions/Validation/` |
| Regras de negócio (reserva) | `Application/Services/ReservationService.cs` |
| Configuração de entidade EF | `Infrastructure/Persistence/Configurations/` |
| Seed data | `Infrastructure/Services/CustomUserSeeder.cs` e `SportSeeder.cs` |
