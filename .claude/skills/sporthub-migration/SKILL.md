---
name: sporthub-migration
description: >
  Guides and runs EF Core migrations for the SportHub project, checking prerequisites and running the correct commands.
  Use this skill whenever the user wants to create, apply, or troubleshoot a migration in SportHub — even if they say "criar migration", "rodar migration", "atualizar banco", "aplicar mudanças no banco", "migration de X", or just ask why the migration failed. Trigger any time EF Core or database schema changes are involved.
---

# SportHub Migration

Handles EF Core migrations for SportHub: validates prerequisites, runs the right commands, and diagnoses common failures.

## Project context

- **EF project:** `src/SportHub.Infrastructure`
- **Startup project:** `src/SportHub.Api`
- **DbContext:** `Infrastructure.Persistence.ApplicationDbContext` (in `AppDbContext.cs`)
- **Configurations dir:** `src/SportHub.Infrastructure/Persistence/Configurations/`
- **Migrations dir:** `src/SportHub.Infrastructure/Migrations/`
- **Repository DI:** `src/SportHub.Api/Extensions/ServiceExtensions.cs`
- **Working directory for all commands:** repo root (`/Users/yurigarciaribeiro/Documents/GitHub/SportHub`)

## Standard commands

**Create a new migration:**
```bash
dotnet ef migrations add <MigrationName> --project src/SportHub.Infrastructure --startup-project src/SportHub.Api
```

**Apply migrations to database:**
```bash
dotnet ef database update --project src/SportHub.Infrastructure --startup-project src/SportHub.Api
```

**Apply with explicit connection string (when env vars aren't set):**
```bash
ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=SportHubDb;Username=postgres;Password=postgres" dotnet ef database update --project src/SportHub.Infrastructure --startup-project src/SportHub.Api
```

**List migrations:**
```bash
dotnet ef migrations list --project src/SportHub.Infrastructure --startup-project src/SportHub.Api
```

**Remove last migration (only if not applied):**
```bash
dotnet ef migrations remove --project src/SportHub.Infrastructure --startup-project src/SportHub.Api
```

## Pre-flight checklist — run BEFORE creating a migration

1. **Entity exists in Domain** — `src/SportHub.Domain/Entities/{Entity}.cs` must exist
2. **DbSet registered** — `AppDbContext.cs` must have `public DbSet<{Entity}> {Entities} { get; set; } = null!;`
3. **EF configuration exists** — `src/SportHub.Infrastructure/Persistence/Configurations/{Entity}Configuration.cs` must exist (auto-discovered via `ApplyConfigurationsFromAssembly`)
4. **Repository registered in DI** — `ServiceExtensions.cs` must have `builder.Services.AddScoped<I{Entity}Repository, {Entity}Repository>();`
5. **Project builds** — run `dotnet build` first; a migration on a broken build produces confusing errors

## Migration process

### Step 1: Validate prerequisites

Check each item in the pre-flight list above. If any is missing, fix it before proceeding (or use `sporthub-new-entity` skill to generate the missing pieces).

Run `dotnet build` to confirm the project compiles:
```bash
dotnet build
```

### Step 2: Create the migration

Use PascalCase names that describe the schema change clearly:
- `Add{Entity}` — new table
- `Add{Property}To{Entity}` — new column
- `Update{Entity}{Description}` — modified columns/indexes
- `Remove{Property}From{Entity}` — dropped column

```bash
dotnet ef migrations add <MigrationName> --project src/SportHub.Infrastructure --startup-project src/SportHub.Api
```

### Step 3: Review the generated migration

Always read the generated `Up()` and `Down()` methods before applying. Check:
- Expected tables/columns are being created
- No unintended drops or renames
- Foreign keys and indexes look correct

### Step 4: Apply to database

```bash
ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=SportHubDb;Username=postgres;Password=postgres" dotnet ef database update --project src/SportHub.Infrastructure --startup-project src/SportHub.Api
```

## Common failures and fixes

| Error | Cause | Fix |
|---|---|---|
| `Unable to create an object of type 'ApplicationDbContext'` | Build failure or missing design-time factory | Run `dotnet build` first, fix any compile errors |
| `The entity type 'X' requires a primary key` | Missing `HasKey` in configuration | Add `builder.HasKey(e => e.Id)` in `{Entity}Configuration.cs` |
| `No migration named 'X'` | Typo or migration doesn't exist | Run `dotnet ef migrations list` to see available migrations |
| Migration creates unexpected columns/tables | Missing or extra DbSet | Check `AppDbContext.cs` for stray DbSets |
| `Npgsql.PostgresException: table already exists` | Migration was applied manually or DB is out of sync | Check `__EFMigrationsHistory` table; consider `dotnet ef database update <PreviousMigration>` to roll back |
| `An error occurred while accessing the IWebHost` | API project has a startup error | Fix the startup error first (`dotnet run` to see it) |

## Naming conventions

Good migration names describe the schema change, not the feature:
- ✅ `AddTournamentTable`
- ✅ `AddDescriptionToSport`
- ✅ `AddIndexOnReservationStartTime`
- ❌ `UpdateFeature` (too vague)
- ❌ `Fix` (meaningless)
- ❌ `Migration1` (no context)
