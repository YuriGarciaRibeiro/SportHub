---
name: sporthub-scaffold
description: >
  Scaffolds complete CQRS feature files for the SportHub project following its Clean Architecture conventions.
  Use this skill whenever the user wants to add a new UseCase, operation, endpoint, or feature to SportHub — even if they just say "adicionar X", "criar endpoint para Y", "novo command de Z", "implementar feature de W", or describe a new capability. Trigger whenever SportHub feature development is involved, not just when the user explicitly says "scaffold".
---

# SportHub Scaffold

Generates all boilerplate files for a new feature in SportHub, following the project's exact conventions.

## Project conventions (must follow exactly)

**Namespaces:**
- Application layer: `Application.UseCases.{Feature}.{Action}{Entity}`
- Infrastructure configurations: `Infrastructure.Persistence.Configurations`
- API endpoints: `SportHub.Api.Endpoints`

**File locations:**
- Commands/Queries/Handlers/Validators/Responses: `src/SportHub.Application/UseCases/{Feature}/{Action}{Entity}/`
- Entity configurations: `src/SportHub.Infrastructure/Persistence/Configurations/`
- Endpoints: `src/SportHub.Api/Endpoints/{Entity}Endpoints.cs` (add to existing file, or create new)

**CQRS interfaces:**
- `ICommand<TResponse>` / `ICommand` (no response) → `ICommandHandler<TCommand, TResponse>`
- `IQuery<TResponse>` → `IQueryHandler<TQuery, TResponse>`
- All handlers return `Result<T>` or `Result` (FluentResults)

**Base classes:**
- Tenant-scoped entities extend `TenantEntity` (has `TenantId`)
- All entities extend `AuditEntity` (has `Id`, `CreatedAt/By`, `UpdatedAt/By`, `IsDeleted`)

**Errors:** Use typed errors from `Application.Common.Errors` — `NotFound`, `Conflict`, `Unauthorized`, `Forbidden`, `BadRequest`

**Endpoint pattern:**
```csharp
group.MapPost("/", async ([FromBody] XxxCommand command, ISender sender) =>
{
    var result = await sender.Send(command);
    return result.ToIResult();
})
.WithName("OperationName")
.WithSummary("Descrição em português")
.RequireAuthorization(PolicyNames.IsOwner) // or IsManager, IsStaff, or omit for public
.Produces<ResponseType>(StatusCodes.Status200OK)
.Produces<ProblemDetails>(StatusCodes.Status422UnprocessableEntity);
```

**Authorization policies:** `IsStaff`, `IsManager`, `IsOwner`, `IsSuperAdmin`

## Scaffold process

When the user asks to create a new feature or operation, follow these steps:

### 1. Clarify intent (ask if not clear)
- Entity name (e.g., `Member`, `Tournament`, `Promotion`)
- Operation type: **Command** (creates/updates/deletes data) or **Query** (reads data)
- Action name (e.g., `Create`, `Update`, `Delete`, `Get`, `GetAll`, `Toggle`)
- Return type: `Guid` (new ID), a Response DTO, `PagedResult<T>`, or void
- Authorization level: public, authenticated, IsStaff, IsManager, IsOwner, IsSuperAdmin
- Is the entity tenant-scoped? (almost always yes)

### 2. Generate Application layer files

**For Commands** — generate these files in `src/SportHub.Application/UseCases/{Feature}/{Action}{Entity}/`:

`{Action}{Entity}Command.cs`:
```csharp
using Application.CQRS;

namespace Application.UseCases.{Feature}.{Action}{Entity};

public class {Action}{Entity}Command : ICommand<{ReturnType}>
{
    // properties matching request body
}
```

`{Action}{Entity}Handler.cs`:
```csharp
using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.CQRS;

namespace Application.UseCases.{Feature}.{Action}{Entity};

public class {Action}{Entity}Handler : ICommandHandler<{Action}{Entity}Command, {ReturnType}>
{
    private readonly I{Entity}Repository _{entityLower}Repository;
    private readonly IUnitOfWork _unitOfWork;

    public {Action}{Entity}Handler(I{Entity}Repository {entityLower}Repository, IUnitOfWork unitOfWork)
    {
        _{entityLower}Repository = {entityLower}Repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<{ReturnType}>> Handle({Action}{Entity}Command request, CancellationToken cancellationToken)
    {
        // TODO: implement business logic
        // Use Result.Fail(new NotFound(...)) for errors
        // Use Result.Ok(value) for success
    }
}
```

`{Action}{Entity}Validator.cs`:
```csharp
using FluentValidation;

namespace Application.UseCases.{Feature}.{Action}{Entity};

public class {Action}{Entity}Validator : AbstractValidator<{Action}{Entity}Command>
{
    public {Action}{Entity}Validator()
    {
        // TODO: add validation rules
        // RuleFor(x => x.Field).NotEmpty();
        // RuleFor(x => x.Name).MaximumLength(100);
    }
}
```

`{Action}{Entity}Response.cs` (only if returning a DTO, not Guid/void):
```csharp
namespace Application.UseCases.{Feature}.{Action}{Entity};

public class {Action}{Entity}Response
{
    // response properties
}
```

**For Queries** — same folder, but use `IQuery<TResponse>` and `IQueryHandler`:

```csharp
// Query (prefer record for simple queries)
public record Get{Entity}Query(Guid Id) : IQuery<{Entity}Response>;

// Handler
public class Get{Entity}Handler : IQueryHandler<Get{Entity}Query, {Entity}Response>
{
    private readonly I{Entity}Repository _{entityLower}Repository;

    public Get{Entity}Handler(I{Entity}Repository {entityLower}Repository)
    {
        _{entityLower}Repository = {entityLower}Repository;
    }

    public async Task<Result<{Entity}Response>> Handle(Get{Entity}Query request, CancellationToken cancellationToken)
    {
        // TODO: implement query
    }
}
```

### 3. Generate Infrastructure layer (only for new entities)

If this is a **new entity** (not just a new operation on an existing one), generate:

`src/SportHub.Infrastructure/Persistence/Configurations/{Entity}Configuration.cs`:
```csharp
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class {Entity}Configuration : IEntityTypeConfiguration<{Entity}>
{
    public void Configure(EntityTypeBuilder<{Entity}> builder)
    {
        builder.HasKey(e => e.Id);

        // TODO: add property constraints, indexes, relationships
        // builder.Property(e => e.Name).IsRequired().HasMaxLength(100);
        // builder.HasIndex(e => e.TenantId);
    }
}
```

Also remind the user to:
1. Add the entity to `AppDbContext` as a `DbSet<{Entity}>`
2. Run: `dotnet ef migrations add Add{Entity} --project src/SportHub.Infrastructure --startup-project src/SportHub.Api`

### 4. Generate API layer snippet

Show the endpoint code to add to the appropriate `{Entity}Endpoints.cs`:

```csharp
// Add this using at the top:
using Application.UseCases.{Feature}.{Action}{Entity};

// Add this route inside Map{Entity}Endpoints():
group.Map{HttpVerb}("/{route}", async ([FromBody] {Action}{Entity}Command command, ISender sender) =>
{
    var result = await sender.Send(command);
    return result.ToIResult();
})
.WithName("{Action}{Entity}")
.WithSummary("TODO: descrição em português")
.RequireAuthorization(PolicyNames.{Policy})
.Produces<{ReturnType}>(StatusCodes.Status200OK)
.Produces<ProblemDetails>(StatusCodes.Status422UnprocessableEntity);
```

### 5. Write all files and summarize

- Write all generated files directly to disk
- After writing, print a summary table:

```
✓ Arquivos criados:
  src/SportHub.Application/UseCases/{Feature}/{Action}{Entity}/{Action}{Entity}Command.cs
  src/SportHub.Application/UseCases/{Feature}/{Action}{Entity}/{Action}{Entity}Handler.cs
  src/SportHub.Application/UseCases/{Feature}/{Action}{Entity}/{Action}{Entity}Validator.cs
  [src/SportHub.Application/UseCases/{Feature}/{Action}{Entity}/{Action}{Entity}Response.cs]
  [src/SportHub.Infrastructure/Persistence/Configurations/{Entity}Configuration.cs]

⚠ Ações manuais necessárias:
  1. Implementar a lógica no Handler (marcado com TODO)
  2. Adicionar regras de validação no Validator
  3. [Se nova entidade] Adicionar DbSet<{Entity}> no AppDbContext
  4. [Se nova entidade] Criar migration: dotnet ef migrations add Add{Entity} --project src/SportHub.Infrastructure --startup-project src/SportHub.Api
  5. Adicionar o snippet de endpoint em src/SportHub.Api/Endpoints/{Entity}Endpoints.cs
```

## Key things to get right

- **Namespace vs project name**: Application layer uses `Application.UseCases.*` (not `SportHub.Application`), Infrastructure uses `Infrastructure.*`, only API uses `SportHub.Api.*`
- **Always inject IUnitOfWork** in command handlers and call `await _unitOfWork.SaveChangesAsync(cancellationToken)` after mutations
- **Queries never modify data** and don't need IUnitOfWork
- **No EF Core in handlers** — always go through repository interfaces (`IXxxRepository`) defined in `Application.Common.Interfaces`
- **Validation is automatic** via `ValidationBehavior` pipeline — no need to call validator manually in handlers
- **`using` for `Result`** comes from FluentResults, already available as `using FluentResults;` (or via global usings — check if needed)
