---
name: sporthub-new-entity
description: >
  Scaffolds a complete new entity for the SportHub project: Domain entity, repository interface, repository implementation, EF configuration, and DI registration — following all project conventions.
  Use this skill whenever the user wants to add a new entity, model, or table to SportHub — even if they say "criar entidade X", "preciso de uma tabela para Y", "adicionar modelo Z", "novo domínio de W". Trigger any time a new domain concept needs to be persisted, not just when the user says "new entity".
---

# SportHub New Entity

Generates all infrastructure files for a brand-new domain entity in SportHub, from the Domain class all the way through to DI registration.

## Project conventions (must follow exactly)

**Namespaces:**
- Domain entities: `Domain.Entities`
- Domain base classes: `SportHub.Domain.Common` (AuditEntity) and `Domain.Common` (TenantEntity — imports `Domain.Entities` for the Tenant nav property)
- Repository interface: `Application.Common.Interfaces`
- Repository implementation: `Infrastructure.Repositories`
- EF configuration: `Infrastructure.Persistence.Configurations`

**File locations:**
- Entity: `src/SportHub.Domain/Entities/{Entity}.cs`
- Repository interface: `src/SportHub.Application/Common/Interfaces/I{Entity}Repository.cs`
- Repository implementation: `src/SportHub.Infrastructure/Repositories/{Entity}Repository.cs`
- EF configuration: `src/SportHub.Infrastructure/Persistence/Configurations/{Entity}Configuration.cs`
- DI registration: add to `src/SportHub.Api/Extensions/ServiceExtensions.cs`
- DbSet: add to `src/SportHub.Infrastructure/Persistence/AppDbContext.cs`

**Base classes:**
- Tenant-scoped entities (most cases): `public class {Entity} : TenantEntity, IEntity` — gives `TenantId`, `Tenant` nav, plus all audit fields
- Non-tenant entities (rare, e.g. platform-level): `public class {Entity} : AuditEntity, IEntity` — gives audit fields only
- `IEntity` requires `public Guid Id { get; set; }`

**DbSet pattern in AppDbContext:**
```csharp
public DbSet<{Entity}> {Entities} { get; set; } = null!;
```
EF configurations are applied automatically via `builder.ApplyConfigurationsFromAssembly(...)` — no manual registration needed.

**Repository DI registration in ServiceExtensions.cs:**
```csharp
builder.Services.AddScoped<I{Entity}Repository, {Entity}Repository>();
```

## Scaffold process

### 1. Clarify intent (ask if not clear)
- Entity name (PascalCase singular, e.g. `Tournament`, `Promotion`, `MemberPlan`)
- Is it tenant-scoped? (yes for almost everything — only say no for platform-level concepts)
- Key properties and their types
- Any relationships to existing entities (`Court`, `User`, `Location`, `Sport`, `Reservation`)
- Soft-delete needed? (always yes — `AuditEntity` provides it automatically via global EF filter)

### 2. Generate Domain entity

`src/SportHub.Domain/Entities/{Entity}.cs`:
```csharp
using Domain.Common;
using SportHub.Domain.Common;

namespace Domain.Entities;

public class {Entity} : TenantEntity, IEntity
{
    public Guid Id { get; set; }

    // Required properties
    public string Name { get; set; } = null!;

    // Optional properties with sensible defaults
    // public string? Description { get; set; }

    // Navigation properties
    // public Guid RelatedEntityId { get; set; }
    // public RelatedEntity? RelatedEntity { get; set; }
}
```

### 3. Generate Repository interface

`src/SportHub.Application/Common/Interfaces/I{Entity}Repository.cs`:
```csharp
using Domain.Entities;

namespace Application.Common.Interfaces;

public interface I{Entity}Repository
{
    Task<{Entity}?> GetByIdAsync(Guid id);
    Task<List<{Entity}>> GetAllAsync();
    Task AddAsync({Entity} entity);
    Task UpdateAsync({Entity} entity);
    Task RemoveAsync({Entity} entity);
    Task<bool> ExistsAsync(Guid id);
    IQueryable<{Entity}> Query();
}
```

Add extra methods if the entity's likely queries suggest them (e.g. `GetByCourtIdAsync`, `GetPagedAsync` for list-heavy entities).

### 4. Generate Repository implementation

`src/SportHub.Infrastructure/Repositories/{Entity}Repository.cs`:
```csharp
using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class {Entity}Repository : I{Entity}Repository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly DbSet<{Entity}> _dbSet;

    public {Entity}Repository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
        _dbSet = dbContext.Set<{Entity}>();
    }

    public async Task<{Entity}?> GetByIdAsync(Guid id)
        => await _dbSet.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);

    public async Task<List<{Entity}>> GetAllAsync()
        => await _dbSet.AsNoTracking().ToListAsync();

    public Task AddAsync({Entity} entity)
    {
        _dbSet.Add(entity);
        return Task.CompletedTask;
    }

    public Task UpdateAsync({Entity} entity)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public Task RemoveAsync({Entity} entity)
    {
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(Guid id)
        => await _dbSet.AnyAsync(e => e.Id == id);

    public IQueryable<{Entity}> Query()
        => _dbSet.AsQueryable();
}
```

### 5. Generate EF Core configuration

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

        // Property constraints
        // builder.Property(e => e.Name).IsRequired().HasMaxLength(100);

        // Indexes
        builder.HasIndex(e => e.TenantId);

        // Relationships
        // builder.HasOne(e => e.RelatedEntity)
        //     .WithMany(r => r.{Entities})
        //     .HasForeignKey(e => e.RelatedEntityId)
        //     .OnDelete(DeleteBehavior.Cascade);
    }
}
```

### 6. Edit AppDbContext — add DbSet

Open `src/SportHub.Infrastructure/Persistence/AppDbContext.cs` and add after the last DbSet:
```csharp
public DbSet<{Entity}> {Entities} { get; set; } = null!;
```

### 7. Edit ServiceExtensions.cs — register repository

Open `src/SportHub.Api/Extensions/ServiceExtensions.cs` and add after the last repository registration:
```csharp
builder.Services.AddScoped<I{Entity}Repository, {Entity}Repository>();
```

### 8. Print summary and next steps

```
✓ Arquivos criados:
  src/SportHub.Domain/Entities/{Entity}.cs
  src/SportHub.Application/Common/Interfaces/I{Entity}Repository.cs
  src/SportHub.Infrastructure/Repositories/{Entity}Repository.cs
  src/SportHub.Infrastructure/Persistence/Configurations/{Entity}Configuration.cs

✓ Arquivos editados:
  src/SportHub.Infrastructure/Persistence/AppDbContext.cs  ← DbSet<{Entity}> adicionado
  src/SportHub.Api/Extensions/ServiceExtensions.cs        ← DI registration adicionado

⚠ Próximos passos:
  1. Revisar e completar propriedades da entidade
  2. Completar o EntityConfiguration (constraints, indexes, relationships)
  3. Criar migration:
     dotnet ef migrations add Add{Entity} --project src/SportHub.Infrastructure --startup-project src/SportHub.Api
  4. Usar sporthub-scaffold para adicionar UseCases (Commands/Queries) para esta entidade
```

## Key things to get right

- `TenantEntity` already has `TenantId` set automatically by `SaveChangesAsync` — never set it manually in handlers
- `AuditEntity` fields (`CreatedAt`, `UpdatedAt`, `IsDeleted`, etc.) are also set automatically — never set them manually
- The global EF query filter for `IsDeleted` is applied automatically — soft-deleted records are invisible by default
- `ApplyConfigurationsFromAssembly` picks up the new `IEntityTypeConfiguration` automatically — no manual `modelBuilder.Entity<>()` calls needed
- `RemoveAsync` triggers a hard delete — for soft delete, call `entity.MarkAsDeleted(userId)` and then `SaveChangesAsync` instead
