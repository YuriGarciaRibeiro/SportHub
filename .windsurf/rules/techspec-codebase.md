---
trigger: model_decision
description: Usar sempre que precisar tomar decisão arquitetural ou técnica no projeto SportHub
---

# Regra: Consultar Tech Spec do Codebase

Antes de tomar qualquer decisão arquitetural, criar novos componentes ou modificar padrões existentes, **consulte obrigatoriamente** o arquivo `documentos/techspec-codebase.md` que contém a documentação técnica completa do projeto SportHub.

## Stack e Arquitetura

- **.NET 10 (Preview)** com **Minimal APIs** e **Clean Architecture** (4 camadas: Domain → Application → Infrastructure → Api)
- **PostgreSQL 16** multi-schema (schema-per-tenant) + **Redis 7** (cache) + **EF Core 9**
- **CQRS** via MediatR 13 com interfaces customizadas (`ICommand`/`IQuery`/`ICommandHandler`/`IQueryHandler`)
- **Result Pattern** via FluentResults 4.0 (nunca lançar exceções para erros de negócio)
- **FluentValidation 12** com `ValidationBehavior` no pipeline MediatR
- **JWT Bearer** (HMAC-SHA256, 2h hardcoded) + Authorization Policies (IsStaff, IsManager, IsOwner, IsSuperAdmin)

## Padrões Obrigatórios

1. **UseCases**: pasta `UseCases/{Domínio}/{Ação}/` com Command/Query (record) + Handler + Validator
2. **Handlers retornam `Result<T>`** — use `Result.Ok(...)` ou `Result.Fail(new ErrorType("msg"))`
3. **Erros tipados**: `BadRequest` (422!), `NotFound` (404), `Unauthorized` (401), `Conflict` (409), `Forbidden` (403)
4. **Endpoints**: static class com extension method, usar `ISender` + `.ToIResult()`, registrar em `AppExtensions`
5. **Entidades**: herdar `AuditEntity` + implementar `IEntity`, criar Configuration EF, DbSet, Repository + interface
6. **DI manual**: registrar tudo explicitamente em `ServiceExtensions` (AddServices/AddRepositories)
7. **Tenant-aware**: dados de tenant via `ApplicationDbContext` (schema resolvido pelo interceptor)

## Gotchas Críticos

- `BadRequest` = HTTP **422** (não 400)
- `JwtService` ignora `ExpiryMinutes` do appsettings (hardcoded 2h)
- `Tenant` NÃO implementa `IEntity` nem `AuditEntity` — repo separado com `TenantDbContext`
- `BaseRepository` faz `SaveChangesAsync` por operação (sem Unit of Work)
- `CacheService` está em Application mas com namespace `Infrastructure.Services`
- Soft Delete automático no `SaveChangesAsync` do `ApplicationDbContext`
- Schema dinâmico via `SET search_path` no `TenantSchemaConnectionInterceptor`

## Referência Completa

Para detalhes sobre modelo de dados, mapa de navegação, pipeline de middleware, provisioning de tenants e guia de padronização, consulte: **`documentos/techspec-codebase.md`**
