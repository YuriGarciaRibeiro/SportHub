# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Build
dotnet build

# Run API (from repo root)
cd src/SportHub.Api && dotnet run

# Run all tests
dotnet test

# Run a single test class
dotnet test --filter "FullyQualifiedName~TestClassName"

# Run with Docker (recommended for full stack)
docker-compose up -d

# EF Core migrations (run from repo root)
dotnet ef migrations add <MigrationName> --project src/SportHub.Infrastructure --startup-project src/SportHub.Api
dotnet ef database update --project src/SportHub.Infrastructure --startup-project src/SportHub.Api
```

## Infrastructure (Docker)

| Service | URL | Credentials |
|---|---|---|
| API | http://localhost:5001 | - |
| API docs (Scalar) | http://localhost:5001/scalar/v1 | - |
| PostgreSQL | localhost:5432 | postgres / postgres |
| pgAdmin | http://localhost:5050 | admin@admin.com / admin |
| Redis | localhost:6379 | - |
| Redis Commander | http://localhost:8081 | - |
| MinIO | http://localhost:9000 | minioadmin / minioadmin |
| MinIO Console | http://localhost:9001 | minioadmin / minioadmin |

For local dev without Docker, use `X-Tenant-Slug` header to identify tenant (in production, resolved from subdomain).

## Architecture

Clean Architecture with 4 layers (target framework: net10.0):

- **SportHub.Domain** — Entities, value objects, enums. No dependencies on other layers.
- **SportHub.Application** — Use cases (CQRS via MediatR), interfaces, FluentValidation validators, FluentResults-based error types.
- **SportHub.Infrastructure** — EF Core (Npgsql), repositories, Redis cache, SignalR hubs, S3/MinIO storage, security handlers, seeders.
- **SportHub.Api** — Minimal API endpoints (no controllers), middleware, DI wiring.

### Multi-tenancy

The system is multi-tenant. Every request is scoped to a `Tenant` resolved by `TenantResolutionMiddleware` before authentication. Resolution order:
1. Subdomain: `abc.sporthub.app` → slug `abc`
2. Header `X-Tenant-Slug` (development fallback)

Resolved tenant is stored in `ITenantContext` (scoped). Tenant data is cached in Redis for 1 hour. `User`, `Court`, `Sport`, `Reservation`, `Location` are all tenant-scoped via `TenantId`. `Tenant` itself lives in the shared public schema.

Paths that bypass tenant resolution: `/api/tenants/**`, `/auth/register`, `/auth/me`, `/health`, `/scalar/**`, `/openapi/**`, `/hubs/**`. Paths with optional tenant (won't block if absent): `/auth/login`, `/auth/refresh`, `/api/branding`.

### CQRS Pattern

Commands return `Result` or `Result<T>` (FluentResults). Queries follow the same pattern.

```
ICommand<TResponse>      → ICommandHandler<TCommand, TResponse>
IQuery<TResponse>        → IQueryHandler<TQuery, TResponse>
```

All handlers go in `Application/UseCases/<Feature>/<Action>/` with co-located Command/Query, Handler, Validator, and Response files.

Pipeline behaviors (ordered):
1. `ValidationBehavior<,>` — runs FluentValidation, returns validation errors as `Result` failures
2. `LoggingBehavior<,>` — logs request/response

### Error Handling

Errors are typed classes in `Application/Common/Errors/` (e.g., `NotFound`, `Conflict`, `Unauthorized`, `Forbidden`, `BadRequest`). Each carries an HTTP status code in its `Metadata`. Endpoints call `.ToIResult()` on the `Result` to convert to `IResult` with RFC 9457 problem details.

### Domain Entities

- `AuditEntity` — base for soft-deletable entities; has `CreatedAt/By`, `UpdatedAt/By`, `IsDeleted`, `DeletedAt/By`. Soft-delete is enforced as a global EF query filter.
- `Tenant` — uses static factory `Tenant.Create(...)`, all mutations via methods (`Suspend()`, `Activate()`, `UpdateSettings()`, etc.)
- `UserRole` enum: `Customer=0`, `Staff=1`, `Manager=2`, `Owner=3`, `SuperAdmin=99`

### Authorization

Four policies in `PolicyNames`:
- `IsStaff`, `IsManager`, `IsOwner` — checked by `GlobalRoleHandler` against the user's role in the current tenant
- `IsSuperAdmin` — checked by `SuperAdminHandler`, platform-level operator (no tenant required)

### Endpoints

All routes use Minimal API style, registered in `Api/Endpoints/*Endpoints.cs` and wired in `AppExtensions.UseEndpoints()`.

### Storage

`IStorageService` abstracts file uploads. Uses MinIO locally and AWS S3 in production, configured via `Storage__*` env vars.

### Seeding

On startup: `SuperAdminSeeder` runs unconditionally. On tenant provisioning (`TenantProvisioningService.ProvisionAsync`): seeds default sports, owner user (default password `Owner@123`), and a default location.
