---
trigger: model_decision
description: Usar sempre que precisar tomar decisão arquitetural ou técnicas
---

# Regra: Consultar Tech Spec do Codebase

Antes de tomar qualquer decisão arquitetural, técnica ou de design de código no projeto **SportHub**, consulte o documento completo de especificação técnica em `documentos/techspec-codebase.md`.

## Stack e Arquitetura

- **.NET 9** (C# 13) — ASP.NET Core **Minimal APIs**
- **Clean Architecture** com 4 camadas: `Domain` → `Application` → `Infrastructure` → `Api`
- **CQRS** via MediatR 13 com `ICommand<T>`/`IQuery<T>` retornando `Result<T>` (FluentResults 4.0)
- **FluentValidation** 12 com `ValidationBehavior` no pipeline MediatR
- **PostgreSQL 16** (EF Core 9 + Npgsql) + **Redis 7** (IDistributedCache)
- **JWT Bearer** (HMAC-SHA256) com Authorization Policies por estabelecimento

## Regras Obrigatórias ao Criar/Modificar Código

### Use Cases (Application Layer)
- Cada use case em pasta própria: `UseCases/{Domínio}/{Feature}/`
- Arquivos: `{Feature}Command.cs` ou `{Feature}Query.cs` + `{Feature}Handler.cs` + `{Feature}Validator.cs` + `{Feature}Response.cs`
- Commands implementam `ICommand<T>`, Queries implementam `IQuery<T>`
- Handlers implementam `ICommandHandler<,>` ou `IQueryHandler<,>`
- Retornar `Result.Ok(...)` ou `Result.Fail(new {ErrorType}("..."))` — **nunca lançar exceções**

### Erros Tipados (Application/Common/Errors/)
- `NotFound` → 404 | `BadRequest` → **422** | `Conflict` → 409 | `Forbidden` → 403 | `Unauthorized` → 401
- Erros genéricos `Result.Fail("msg")` resultam em status 400

### Entidades (Domain Layer)
- Herdar `AuditEntity` + implementar `IEntity` (exceto tabelas de junção)
- Soft delete automático via `SaveChangesAsync` — nunca deletar fisicamente
- Value Objects como Owned Entities no EF Core

### Repositórios (Infrastructure Layer)
- Herdar `BaseRepository<T>` (constraint: `IEntity`)
- Interface em `Application/Common/Interfaces/`, implementação em `Infrastructure/Repositories/`
- Usar `AsSplitQuery()` + `AsNoTracking()` em queries de leitura com Include
- Registrar DI em `ServiceExtensions.AddRepositories()`

### Endpoints (Api Layer)
- Minimal API com `MapGroup` em classes estáticas `{Entidade}Endpoints`
- Usar `ISender` + `result.ToIResult()` para conversão automática para ProblemDetails
- Registrar em `AppExtensions.UseEndpoints()`
- Documentar com `.WithName()`, `.WithSummary()`, `.Produces<>()`

### Cache (Redis)
- Usar `ICacheService.GenerateCacheKey(CacheKeyPrefix.X, ...)` para chaves
- TTL padrão: 30 minutos
- Novos prefixos em `Application/Common/Enums/CacheKeyPrefix.cs`

### Validação
- Um `AbstractValidator<TCommand>` por Command, no mesmo diretório
- Registrado automaticamente — nunca chamar validators manualmente
- Extensão `Password()` disponível para validação de senha

### Gotchas Críticos
1. `BadRequest` retorna **422**, não 400
2. `JwtService` ignora `ExpiryMinutes` do appsettings (hardcoded 2h)
3. `EstablishmentUser` não implementa `IEntity` (chave composta)
4. `EstablishmentHandler` dá bypass para `Admin` **e** `User` — só verifica `EstablishmentMember`
5. `BaseRepository.AddAsync` faz commit imediato (SaveChanges por operação)
6. `SportsEndpoints` usa prefixo `/api/sports` (diferente dos demais)

## Referência Completa
Consulte `documentos/techspec-codebase.md` para detalhes sobre: relações de entidades, fluxo de reservas, configuração de quadras, seeders, pipeline behaviors, configuração de DI e mapa de navegação.
