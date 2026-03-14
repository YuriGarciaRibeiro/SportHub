---
trigger: model_decision
description: Usar sempre que precisar tomar decisão arquitetural ou técnica no backend SportHub
---

# Regra: Consultar Tech Spec do Codebase

Antes de tomar qualquer decisão arquitetural, criar novos arquivos, modificar padrões existentes ou responder perguntas técnicas sobre o backend SportHub, **consulte obrigatoriamente** o documento `documentos/techspec-codebase.md`.

## Quando usar

- Criar novos UseCases (Commands, Queries, Handlers, Validators)
- Adicionar endpoints Minimal API
- Criar/modificar entidades, repositórios ou serviços
- Implementar autorização (policies, handlers)
- Tratar erros (Result Pattern, ProblemDetails)
- Trabalhar com multi-tenancy (schemas, middleware, provisioning)
- Registrar dependências no DI container
- Qualquer decisão sobre padrões de código ou convenções

## Stack resumida

- **.NET 9** — Minimal APIs, Clean Architecture (4 camadas: Domain → Application → Infrastructure → Api)
- **CQRS** via MediatR 13 com interfaces customizadas (`ICommand<T>`, `IQuery<T>`, `ICommandHandler`, `IQueryHandler`)
- **Result Pattern** via FluentResults 4.0 — handlers nunca lançam exceptions para erros de negócio
- **FluentValidation 12** — validators co-localizados com Commands/Queries, executados via `ValidationBehavior`
- **PostgreSQL 16** multi-schema (schema-per-tenant) + **Redis 7** para cache
- **JWT Bearer** (HMAC-SHA256, 2h hardcoded)
- **EF Core 9** com Fluent API configurations, soft delete automático via `AuditEntity`

## Padrões obrigatórios

1. **UseCase = pasta** em `Application/UseCases/{Feature}/{Ação}/` com Command/Query + Handler + Validator
2. **Erros tipados**: `BadRequest(422)`, `NotFound(404)`, `Conflict(409)`, `Unauthorized(401)`, `Forbidden(403)`
3. **Endpoints**: usar `result.ToIResult()` para converter Result → HTTP, com `.Produces<T>()` e `.RequireAuthorization()`
4. **DI**: Scoped para services/repos. Registrar em `ServiceExtensions.cs`
5. **Entidades**: herdar `AuditEntity` + `IEntity` (exceto `EstablishmentUser` que tem chave composta e `Tenant` que é isolado)
6. **Tenant**: `TenantDbContext` (public/singleton) vs `ApplicationDbContext` (schema dinâmico/scoped)

## Gotchas críticos

- `BadRequest` = HTTP 422, não 400
- `JwtService` ignora `ExpiryMinutes` — expiração é 2h hardcoded
- `EstablishmentUser` sem `IEntity` (chave composta) — não usa `BaseRepository`
- `EstablishmentHandler` faz bypass para `Admin` e `User` roles
- `BaseRepository` chama `SaveChanges` por operação (sem Unit of Work)
- Middleware order: `TenantResolution → Authentication → Authorization → Endpoints`
- `TenantModelCacheKeyFactory` é essencial para schema dinâmico funcionar

## Referência completa

Toda a documentação detalhada está em **`documentos/techspec-codebase.md`** incluindo:
modelo de dados, endpoints, mapa de navegação de arquivos, guia para novos devs e padrões de código.
