---
trigger: model_decision
description: Usar sempre que precisar tomar decisão arquitetural ou técnica no projeto SportHub
---

# Regra: Consultar Tech Spec do Codebase

Antes de tomar qualquer decisão arquitetural, criar novos componentes ou modificar padrões existentes, **consulte obrigatoriamente** o arquivo `documentos/techspec-codebase.md` (v5.0) que contém a documentação técnica completa do projeto SportHub.

## Stack e Arquitetura

- **.NET 10 (Preview)** com **Minimal APIs** e **Clean Architecture** (4 camadas: Domain → Application → Infrastructure → Api)
- **PostgreSQL 16** multi-schema (schema-per-tenant via `HasDefaultSchema`) + **Redis 7** (cache) + **EF Core 9.0.7**
- **CQRS** via MediatR 13 com interfaces customizadas (`ICommand`/`IQuery`/`ICommandHandler`/`IQueryHandler`)
- **Result Pattern** via FluentResults 4.0 (nunca lançar exceções para erros de negócio)
- **FluentValidation 12** com `ValidationBehavior` no pipeline MediatR
- **JWT Bearer** (HMAC-SHA256, ExpiryMinutes configurável com fallback 2h) + Authorization Policies
- **Unit of Work**: `ApplicationDbContext` implementa `IUnitOfWork` — handlers chamam `SaveChangesAsync()`

## Padrões Obrigatórios

1. **UseCases**: pasta `UseCases/{Domínio}/{Ação}/` com Command/Query (record) + Handler + Validator
2. **Handlers retornam `Result<T>`** — use `Result.Ok(...)` ou `Result.Fail(new ErrorType("msg"))`
3. **Erros tipados**: `BadRequest` (422!), `NotFound` (404), `Unauthorized` (401), `Conflict` (409), `Forbidden` (403)
4. **Endpoints**: static class com extension method, usar `ISender` + `.ToIResult()`, registrar em `AppExtensions`
5. **Entidades**: herdar `AuditEntity` + implementar `IEntity`, criar Configuration EF, DbSet, Repository + interface
6. **DI manual**: registrar tudo explicitamente em `ServiceExtensions` (AddServices/AddRepositories)
7. **Tenant-aware**: dados de tenant via `ApplicationDbContext` (schema via `HasDefaultSchema` no `OnModelCreating`)
8. **Unit of Work**: repositórios NÃO fazem SaveChanges — handler chama `IUnitOfWork.SaveChangesAsync()` ao final

## Gotchas Críticos

- `BadRequest` = HTTP **422** (não 400) — 400 real só para UniqueViolation do PostgreSQL
- `Tenant` NÃO implementa `IEntity` nem `AuditEntity` — repo separado com `TenantDbContext`
- Soft Delete automático no `SaveChangesAsync` — nunca faz DELETE real em `AuditEntity`
- Schema dinâmico via `HasDefaultSchema` no `OnModelCreating` (NÃO usa interceptor `SET search_path`)
- `TenantModelCacheKeyFactory` gera chave por schema — EF Core compila modelo separado por tenant
- Provisioning cria Owner User com senha padrão `Owner@123`
- `/api/branding` e `/api/settings` estão fora do grupo SuperAdmin do `TenantEndpoints`

## Referência Completa

Para detalhes sobre modelo de dados, mapa de navegação, pipeline de middleware, provisioning de tenants e guia de padronização, consulte: **`documentos/techspec-codebase.md`**
