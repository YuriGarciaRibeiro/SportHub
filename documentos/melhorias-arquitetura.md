# Melhorias de Arquitetura — SportHub API

> Documento gerado em: 2026-03-14
> Baseado na análise completa do codebase v4.0

---

## Índice

1. [🔴 Multi-Tenancy: Migrar de `SET search_path` para `HasDefaultSchema`](#1-multi-tenancy-migrar-de-set-search_path-para-hasdefaultschema)
2. [🔴 `IModelCacheKeyFactory`: Incluir schema na chave de cache](#2-imodelcachekeyfactory-incluir-schema-na-chave-de-cache)
3. [🔴 Global Query Filter para Soft Delete](#3-global-query-filter-para-soft-delete)
4. [🟡 Unit of Work: Remover `SaveChanges` dos repositórios](#4-unit-of-work-remover-savechanges-dos-repositórios)
5. [🟡 Centralizar criação de `ApplicationDbContext` para migrations](#5-centralizar-criação-de-applicationdbcontext-para-migrations)
6. [🟡 Mover `CacheService` para Infrastructure](#6-mover-cacheservice-para-infrastructure)
7. [🟡 Converter endpoints sem MediatR em Queries](#7-converter-endpoints-sem-mediatr-em-queries)
8. [🟡 SQL Injection no Interceptor de Schema](#8-sql-injection-no-interceptor-de-schema)
9. [🟢 Usar `TimeProvider` em vez de `DateTime.UtcNow`](#9-usar-timeprovider-em-vez-de-daabortetimeutcnow)
10. [🟢 Usar `ExpiryMinutes` do `JwtSettings`](#10-usar-expiryminutes-do-jwtsettings)

---

## 1. Multi-Tenancy: Migrar de `SET search_path` para `HasDefaultSchema`

### Problema

Atualmente o multi-tenancy funciona via um `DbConnectionInterceptor` que executa `SET search_path TO "tenant_xxx", public;` toda vez que uma conexão é aberta. Essa abordagem tem vários problemas:

**Arquivo atual:** `src/SportHub.Infrastructure/Persistence/Interceptors/TenantSchemaConnectionInterceptor.cs`

```csharp
// CÓDIGO ATUAL (problemático)
private async Task SetSchemaAsync(DbConnection connection, CancellationToken cancellationToken)
{
    var schema = GetCurrentSchema();
    if (schema is null) return;

    await using var command = connection.CreateCommand();
    command.CommandText = $"SET search_path TO \"{schema}\", public;";
    await command.ExecuteNonQueryAsync(cancellationToken);
}
```

**Problemas identificados:**

1. **Lógica duplicada** — O slug do tenant é extraído tanto no `TenantResolutionMiddleware` quanto no `TenantSchemaConnectionInterceptor`, com regras ligeiramente diferentes (o middleware suporta `*.localhost` com 2 partes, o interceptor tem regras separadas).

2. **SQL Injection** — O schema é interpolado diretamente na string SQL sem sanitização. Um header `X-Tenant-Slug` malicioso pode injetar SQL arbitrário.

3. **Connection Pooling** — `SET search_path` altera o estado da conexão PostgreSQL. Se o connection pool reutilizar uma conexão, o `search_path` de um tenant pode "vazar" para outro se o interceptor não for chamado corretamente.

4. **Fonte de verdade paralela** — O interceptor ignora o `ITenantContext` já preenchido pelo middleware, criando duas fontes de verdade para saber qual é o tenant da request.

5. **Migrations ficam complexas** — Sem `HasDefaultSchema`, o EF Core gera SQL sem qualificação de schema. Isso obriga a criar `NpgsqlConnectionStringBuilder` com `SearchPath` manualmente em vários lugares.

### Solução: Usar `HasDefaultSchema` no `OnModelCreating`

A abordagem recomendada pelo EF Core para multi-tenancy schema-per-tenant é configurar o schema diretamente no `OnModelCreating` via `HasDefaultSchema`. O EF Core passa a gerar **todo SQL com schema qualificado** (`"tenant_arena1"."Users"` em vez de `"Users"`).

**Arquivo a alterar:** `src/SportHub.Infrastructure/Persistence/AppDbContext.cs`

```csharp
// CÓDIGO NOVO (recomendado)
using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using SportHub.Domain.Common;

namespace Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ITenantContext _tenantContext;

    public string CurrentSchema => _tenantContext.Schema;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService currentUserService,
        ITenantContext tenantContext)
        : base(options)
    {
        _currentUserService = currentUserService;
        _tenantContext = tenantContext;
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Court> Courts { get; set; } = null!;
    public DbSet<Sport> Sports { get; set; } = null!;
    public DbSet<Reservation> Reservations { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // ✅ NOVO: O EF Core agora qualifica todas as tabelas com o schema do tenant.
        // Ex: SELECT * FROM "tenant_arena1"."Users" WHERE ...
        // Isso elimina a necessidade do TenantSchemaConnectionInterceptor.
        var schema = _tenantContext.Schema;
        if (!string.IsNullOrEmpty(schema))
        {
            builder.HasDefaultSchema(schema);
        }

        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly,
            type => type != typeof(Configurations.TenantConfiguration)
        );
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // ... (mantém a lógica de auditoria e soft delete existente)
        var userId = _currentUserService.UserId;

        foreach (var entry in ChangeTracker.Entries<AuditEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.SetCreated(userId);
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.SetUpdated(userId);
            }
            else if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.MarkAsDeleted(userId);

                foreach (var reference in entry.References)
                {
                    if (reference.TargetEntry != null && reference.TargetEntry.Metadata.IsOwned())
                    {
                        reference.TargetEntry.State = EntityState.Unchanged;
                    }
                }
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
```

**Arquivo a alterar:** `src/SportHub.Api/Extensions/ServiceExtensions.cs` — método `AddDatabase`

```csharp
// CÓDIGO NOVO — remover o interceptor do registro do ApplicationDbContext
public static WebApplicationBuilder AddDatabase(this WebApplicationBuilder builder, IConfiguration configuration)
{
    var connectionString = configuration.GetConnectionString("DefaultConnection");

    // DbContext do schema public (tenants globais) — Scoped
    builder.Services.AddDbContext<TenantDbContext>(options =>
    {
        options.UseNpgsql(connectionString, npgsql =>
            npgsql.MigrationsAssembly("SportHub.Infrastructure")
                  .MigrationsHistoryTable("__EFMigrationsHistory_Global", "public"));
    });

    // DbContext por tenant (schema dinâmico) — Scoped
    // ✅ SEM interceptor — o schema é configurado via HasDefaultSchema no OnModelCreating
    builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
    {
        options.UseNpgsql(connectionString, npgsql =>
            npgsql.MigrationsAssembly("SportHub.Infrastructure")
                  .MigrationsHistoryTable("__EFMigrationsHistory"));
        options.ReplaceService<IModelCacheKeyFactory, TenantModelCacheKeyFactory>();
        options.ConfigureWarnings(w =>
            w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
    });

    return builder;
}
```

**Arquivo a deletar (após migração):** `src/SportHub.Infrastructure/Persistence/Interceptors/TenantSchemaConnectionInterceptor.cs`

### Impacto da mudança

- **Elimina** o `TenantSchemaConnectionInterceptor` inteiro
- **Elimina** a vulnerabilidade de SQL injection
- **Elimina** problemas de connection pooling/search_path leaking
- **Simplifica** o registro de DI (remove `AddScoped<TenantSchemaConnectionInterceptor>`)
- **Exige** atualizar o `TenantModelCacheKeyFactory` (item 2 abaixo)
- **Exige** re-testar as migrations existentes (o SQL gerado agora inclui schema)

---

## 2. `IModelCacheKeyFactory`: Incluir schema na chave de cache

### Problema

Atualmente o `TenantModelCacheKeyFactory` **não inclui o schema** na chave de cache do modelo EF Core:

**Arquivo atual:** `src/SportHub.Infrastructure/Persistence/TenantModelCacheKeyFactory.cs`

```csharp
// CÓDIGO ATUAL (problemático)
public class TenantModelCacheKeyFactory : IModelCacheKeyFactory
{
    public object Create(DbContext context, bool designTime)
    {
        // Não incluímos o schema na chave para evitar crash quando TenantContext
        // ainda não foi resolvido pelo middleware (ex: login, bootstrap do DI).
        return (context.GetType(), designTime);
    }
}
```

Com a abordagem `SET search_path`, isso "funcionava" porque o schema não afetava o modelo compilado — todas as queries eram `SELECT * FROM "Users"` sem qualificação de schema. Porém com `HasDefaultSchema`, o schema faz parte do modelo e **precisa estar na chave de cache**.

Se a chave não incluir o schema, o EF Core vai usar o modelo compilado para o **primeiro tenant** que acessar a API em todas as requests subsequentes. Isso significaria que queries para `tenant_arena1` usariam o schema de `tenant_academia_silva` (ou qualquer que tenha sido o primeiro).

### Solução

```csharp
// CÓDIGO NOVO
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Infrastructure.Persistence;

/// <summary>
/// Garante que o EF Core compile um modelo separado por schema de tenant.
/// Com HasDefaultSchema, cada schema gera SQL diferente, então precisamos
/// de uma chave de cache por schema.
/// </summary>
public class TenantModelCacheKeyFactory : IModelCacheKeyFactory
{
    public object Create(DbContext context, bool designTime)
    {
        var schema = (context as ApplicationDbContext)?.CurrentSchema ?? "public";
        return new TenantModelCacheKey(context.GetType(), schema, designTime);
    }
}

/// <summary>
/// Chave de cache que inclui o schema do tenant para garantir que modelos
/// de schemas diferentes não compartilhem cache.
/// </summary>
internal sealed class TenantModelCacheKey : IEquatable<TenantModelCacheKey>
{
    private readonly Type _contextType;
    private readonly string _schema;
    private readonly bool _designTime;

    public TenantModelCacheKey(Type contextType, string schema, bool designTime)
    {
        _contextType = contextType;
        _schema = schema;
        _designTime = designTime;
    }

    public bool Equals(TenantModelCacheKey? other)
    {
        if (other is null) return false;
        return _contextType == other._contextType
            && _schema == other._schema
            && _designTime == other._designTime;
    }

    public override bool Equals(object? obj) => obj is TenantModelCacheKey other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(_contextType, _schema, _designTime);
}
```

### Trade-off: Memória

Com essa mudança, o EF Core vai manter **um modelo compilado por schema** em memória. Se você tiver 100 tenants, serão ~100 modelos em cache. Cada modelo ocupa ~1-5MB de memória. Para poucos tenants isso é desprezível; para centenas, é algo a monitorar.

A alternativa (se tiver MUITOS tenants) seria limpar periodicamente modelos não usados, mas isso requer customização mais avançada que não é necessária agora.

---

## 3. Global Query Filter para Soft Delete

### Problema

O soft delete é implementado no **write side** (no `SaveChangesAsync` do `ApplicationDbContext`), mas **não há filtro automático no read side**. Isso significa que toda query feita via repositórios retorna registros soft-deleted:

**Arquivo atual:** `src/SportHub.Infrastructure/Repositories/BaseRepository.cs`

```csharp
// CÓDIGO ATUAL — retorna registros soft-deleted!
public async Task<List<T>> GetAllAsync() =>
    await _dbSet.ToListAsync();  // ← inclui IsDeleted = true

public async Task<T?> GetByIdAsync(Guid id) =>
    await _dbSet.FindAsync(id);   // ← pode retornar entidade deletada
```

Cada repositório precisaria adicionar `.Where(x => !x.IsDeleted)` manualmente em cada query, o que é propenso a esquecimentos e bugs.

### Solução: Usar `HasQueryFilter` do EF Core

O EF Core tem um mecanismo nativo chamado **Global Query Filters** que aplica filtros automaticamente em todas as queries de uma entidade. É perfeito para soft delete.

**Arquivo a alterar:** `src/SportHub.Infrastructure/Persistence/AppDbContext.cs` — método `OnModelCreating`

```csharp
protected override void OnModelCreating(ModelBuilder builder)
{
    var schema = _tenantContext.Schema;
    if (!string.IsNullOrEmpty(schema))
    {
        builder.HasDefaultSchema(schema);
    }

    base.OnModelCreating(builder);
    builder.ApplyConfigurationsFromAssembly(
        typeof(ApplicationDbContext).Assembly,
        type => type != typeof(Configurations.TenantConfiguration)
    );

    // ✅ NOVO: Global Query Filter para soft delete.
    // Aplica automaticamente WHERE "IsDeleted" = false em TODAS as queries
    // de entidades que herdam AuditEntity.
    ApplySoftDeleteFilter(builder);
}

/// <summary>
/// Aplica HasQueryFilter(e => !e.IsDeleted) para todas as entidades que herdam AuditEntity.
/// Usa reflexão para descobrir automaticamente — não precisa adicionar manualmente por entidade.
/// </summary>
private static void ApplySoftDeleteFilter(ModelBuilder builder)
{
    foreach (var entityType in builder.Model.GetEntityTypes())
    {
        // Só aplica em entidades que herdam AuditEntity
        if (!typeof(AuditEntity).IsAssignableFrom(entityType.ClrType))
            continue;

        // Cria a expressão: entity => !entity.IsDeleted
        var parameter = Expression.Parameter(entityType.ClrType, "e");
        var property = Expression.Property(parameter, nameof(AuditEntity.IsDeleted));
        var filter = Expression.Lambda(Expression.Not(property), parameter);

        builder.Entity(entityType.ClrType).HasQueryFilter(filter);
    }
}
```

**Import necessário no topo do arquivo:**

```csharp
using System.Linq.Expressions;
```

### Como acessar registros soft-deleted quando necessário

Se em algum caso você **precisa** acessar registros deletados (ex: painel admin de auditoria), use `IgnoreQueryFilters()`:

```csharp
// No repositório ou handler que precisa ver tudo:
var allUsersIncludingDeleted = await _context.Users
    .IgnoreQueryFilters()
    .ToListAsync();
```

### Impacto

- **Todas as queries** passam automaticamente a filtrar `IsDeleted = false`
- **Nenhum repositório** precisa ser alterado — o filtro é transparente
- Para queries que precisem ver deletados, usar `.IgnoreQueryFilters()`
- **Migrations**: o Global Query Filter NÃO afeta o schema do banco — é puramente no lado do EF Core

---

## 4. Unit of Work: Remover `SaveChanges` dos repositórios

### Problema

O `BaseRepository` chama `SaveChangesAsync` **dentro de cada operação individual**:

**Arquivo atual:** `src/SportHub.Infrastructure/Repositories/BaseRepository.cs`

```csharp
// CÓDIGO ATUAL (problemático)
public async Task AddAsync(T entity)
{
    await _dbSet.AddAsync(entity);
    await _context.SaveChangesAsync(); // ← SaveChanges a cada Add!
}

public async Task UpdateAsync(T entity)
{
    _dbSet.Update(entity);
    await _context.SaveChangesAsync(); // ← SaveChanges a cada Update!
}

public async Task RemoveAsync(T entity)
{
    _dbSet.Remove(entity);
    await _context.SaveChangesAsync(); // ← SaveChanges a cada Remove!
}
```

**Problemas:**

1. **Sem atomicidade** — Se um handler faz `AddAsync(user)` e depois `UpdateAsync(refreshToken)`, são 2 transações separadas. Se a segunda falhar, a primeira já foi commitada.
2. **Performance** — Cada `SaveChanges` é um round-trip ao banco. Múltiplas operações no mesmo handler geram múltiplos round-trips desnecessários.
3. **Sem batch** — O EF Core pode otimizar múltiplas operações em um único batch SQL se o `SaveChanges` for chamado uma vez no final. Com a abordagem atual, perde-se essa otimização.

### Solução: Implementar Unit of Work

**Passo 1 — Criar a interface `IUnitOfWork` em Application**

**Arquivo novo:** `src/SportHub.Application/Common/Interfaces/IUnitOfWork.cs`

```csharp
namespace Application.Common.Interfaces;

/// <summary>
/// Abstração do padrão Unit of Work.
/// Commita todas as mudanças pendentes no DbContext de uma vez.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Persiste todas as mudanças pendentes no banco de dados.
    /// Deve ser chamado uma vez no final do handler/use case.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

**Passo 2 — Implementar no `ApplicationDbContext`**

**Arquivo a alterar:** `src/SportHub.Infrastructure/Persistence/AppDbContext.cs`

```csharp
// Adicionar IUnitOfWork à declaração da classe:
public class ApplicationDbContext : DbContext, IUnitOfWork
{
    // ... (tudo existente permanece igual)

    // SaveChangesAsync já existe e já implementa IUnitOfWork implicitamente!
    // Não precisa adicionar nada — apenas o : IUnitOfWork na declaração.
}
```

**Passo 3 — Remover `SaveChangesAsync` dos repositórios**

**Arquivo a alterar:** `src/SportHub.Infrastructure/Repositories/BaseRepository.cs`

```csharp
// CÓDIGO NOVO — repositórios apenas manipulam o ChangeTracker
using Application.Common.Interfaces;
using Domain.Common;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class BaseRepository<T> : IBaseRepository<T> where T : class, IEntity
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public BaseRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id) =>
        await _dbSet.FindAsync(id);

    public async Task<List<T>> GetAllAsync() =>
        await _dbSet.ToListAsync();

    public Task AddAsync(T entity)
    {
        _dbSet.Add(entity);
        return Task.CompletedTask;
        // ✅ SEM SaveChangesAsync — o handler decide quando commitar
    }

    public Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(T entity)
    {
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<List<T>> GetByIdsAsync(IEnumerable<Guid> ids)
    {
        return await _dbSet.Where(entity => ids.Contains(entity.Id)).ToListAsync();
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _dbSet.AnyAsync(entity => entity.Id == id);
    }

    public IQueryable<T> Query()
    {
        return _dbSet.AsQueryable();
    }

    public Task AddManyAsync(IEnumerable<T> entities)
    {
        _dbSet.AddRange(entities);
        return Task.CompletedTask;
    }
}
```

**Passo 4 — Registrar no DI**

**Arquivo a alterar:** `src/SportHub.Api/Extensions/ServiceExtensions.cs`

```csharp
// No método AddServices, adicionar:
builder.Services.AddScoped<IUnitOfWork>(sp =>
    sp.GetRequiredService<ApplicationDbContext>());
```

**Passo 5 — Usar nos handlers**

Exemplo de como um handler MediatR passa a funcionar:

```csharp
// ANTES (sem Unit of Work):
public class RegisterUserHandler : ICommandHandler<RegisterUserCommand, AuthResponse>
{
    private readonly IUsersRepository _usersRepo;

    public async Task<Result<AuthResponse>> Handle(RegisterUserCommand cmd, CancellationToken ct)
    {
        var user = new User { /* ... */ };
        await _usersRepo.AddAsync(user);       // ← SaveChanges interno (1º round-trip)
        await _usersRepo.UpdateAsync(user);     // ← SaveChanges interno (2º round-trip)
        return Result.Ok(response);
    }
}

// DEPOIS (com Unit of Work):
public class RegisterUserHandler : ICommandHandler<RegisterUserCommand, AuthResponse>
{
    private readonly IUsersRepository _usersRepo;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterUserHandler(IUsersRepository usersRepo, IUnitOfWork unitOfWork)
    {
        _usersRepo = usersRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AuthResponse>> Handle(RegisterUserCommand cmd, CancellationToken ct)
    {
        var user = new User { /* ... */ };
        await _usersRepo.AddAsync(user);       // ← Apenas marca no ChangeTracker
        await _usersRepo.UpdateAsync(user);     // ← Apenas marca no ChangeTracker

        await _unitOfWork.SaveChangesAsync(ct); // ← 1 único round-trip, atômico!

        return Result.Ok(response);
    }
}
```

### Nota sobre o `TenantRepository`

O `TenantRepository` usa `TenantDbContext` (não `ApplicationDbContext`), então precisa do próprio Unit of Work ou continuar com `SaveChanges` inline. Como operações de tenant são tipicamente isoladas (criar tenant, atualizar tenant), o impacto é menor. Opcionalmente, crie um `ITenantUnitOfWork` separado.

### Impacto

- **Todos os handlers de Command** precisam injetar `IUnitOfWork` e chamar `SaveChangesAsync` no final
- **Handlers de Query** não são afetados (não fazem write)
- **Atomicidade garantida** — todas as operações de um handler são commitadas ou revertidas juntas
- **Performance** — menos round-trips ao banco

---

## 5. Centralizar criação de `ApplicationDbContext` para migrations

### Problema

Atualmente existem **5+ classes mock** para criar `ApplicationDbContext` fora do pipeline HTTP normal (migrations, provisioning):

- `MigrationTenantContext` (em `ServiceExtensions.cs`)
- `PublicTenantContext` (em `ServiceExtensions.cs`)
- `MigrationCurrentUserService` (em `ServiceExtensions.cs`)
- `StaticTenantContext` (em `TenantProvisioningService.cs`)
- `NullCurrentUserServiceInternal` (em `TenantProvisioningService.cs`)

Todas fazem basicamente a mesma coisa: implementam `ITenantContext` e `ICurrentUserService` com valores estáticos para permitir a criação manual do `ApplicationDbContext`. A lógica de construção do contexto (`NpgsqlConnectionStringBuilder`, `DbContextOptionsBuilder`, `ReplaceService`, `ConfigureWarnings`) é repetida em **3 lugares**.

### Solução: Criar uma Factory

**Arquivo novo:** `src/SportHub.Infrastructure/Persistence/ApplicationDbContextFactory.cs`

```csharp
using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Infrastructure.Persistence;

/// <summary>
/// Factory para criar ApplicationDbContext fora do pipeline HTTP (migrations, provisioning, seeds).
/// Centraliza a configuração que antes estava espalhada em ServiceExtensions e TenantProvisioningService.
/// </summary>
public class ApplicationDbContextFactory
{
    private readonly string _connectionString;

    public ApplicationDbContextFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// Cria um ApplicationDbContext configurado para um schema específico.
    /// Útil para migrations e provisioning de tenants.
    /// </summary>
    /// <param name="schema">Nome do schema PostgreSQL (ex: "tenant_arena1" ou "public")</param>
    /// <param name="migrationsHistorySchema">Schema onde a tabela __EFMigrationsHistory será criada. Se null, usa o schema padrão.</param>
    public ApplicationDbContext Create(string schema, string? migrationsHistorySchema = null)
    {
        var builder = new Npgsql.NpgsqlConnectionStringBuilder(_connectionString)
        {
            SearchPath = schema
        };

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(builder.ConnectionString, npgsql =>
        {
            npgsql.MigrationsAssembly("SportHub.Infrastructure");
            npgsql.MigrationsHistoryTable("__EFMigrationsHistory", migrationsHistorySchema ?? schema);
        });
        optionsBuilder.ReplaceService<IModelCacheKeyFactory, TenantModelCacheKeyFactory>();
        optionsBuilder.ConfigureWarnings(w =>
            w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));

        var tenantContext = new FactoryTenantContext(schema);
        var currentUserService = new FactoryCurrentUserService();

        return new ApplicationDbContext(optionsBuilder.Options, currentUserService, tenantContext);
    }

    /// <summary>
    /// Cria um ApplicationDbContext configurado para um tenant específico.
    /// </summary>
    public ApplicationDbContext CreateForTenant(Tenant tenant)
    {
        var schema = tenant.GetSchemaName();
        return Create(schema, schema);
    }

    /// <summary>
    /// Cria um ApplicationDbContext configurado para o schema public.
    /// </summary>
    public ApplicationDbContext CreateForPublic()
    {
        return Create("public", "public");
    }

    // --- Classes internas (substituem as 5+ classes mock espalhadas) ---

    private sealed class FactoryTenantContext : ITenantContext
    {
        public FactoryTenantContext(string schema) => Schema = schema;

        public Guid TenantId => Guid.Empty;
        public string TenantSlug => Schema;
        public string TenantName => Schema;
        public string? LogoUrl => null;
        public string? PrimaryColor => null;
        public string Schema { get; }
        public bool IsResolved => true;
        public void Resolve(Tenant tenant) { }
    }

    private sealed class FactoryCurrentUserService : ICurrentUserService
    {
        public Guid UserId => Guid.Empty;
    }
}
```

**Uso no `ExecuteMigrations` (simplificado):**

```csharp
public static WebApplication ExecuteMigrations(this WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    // 1. Migrações Globais (TenantDbContext - schema public)
    var tenantDb = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
    tenantDb.Database.Migrate();
    logger.LogInformation("Database principal (Tenants) migrado com sucesso.");

    var connectionString = tenantDb.Database.GetConnectionString();
    if (connectionString is null) return app;

    var factory = new ApplicationDbContextFactory(connectionString);

    // 1.5. Migrações do schema public (ApplicationDbContext)
    using (var publicCtx = factory.CreateForPublic())
    {
        publicCtx.Database.Migrate();
        logger.LogInformation("ApplicationDbContext migrado no schema public.");
    }

    // 2. Migrações dos Tenants
    foreach (var tenant in tenantDb.Tenants.ToList())
    {
        logger.LogInformation("Migrando schema {Schema}", tenant.GetSchemaName());
        using var tenantCtx = factory.CreateForTenant(tenant);
        tenantCtx.Database.Migrate();
    }

    return app;
}
```

**Uso no `TenantProvisioningService` (simplificado):**

```csharp
public async Task ProvisionAsync(Tenant tenant, CancellationToken ct = default)
{
    await _tenantRepository.AddAsync(tenant, ct);

    var schemaName = tenant.GetSchemaName();
    await _globalCtx.Database.ExecuteSqlAsync(
        $"CREATE SCHEMA IF NOT EXISTS \"{schemaName}\"", ct);

    var connectionString = _globalCtx.Database.GetConnectionString()!;
    var factory = new ApplicationDbContextFactory(connectionString);

    await using var tenantDb = factory.CreateForTenant(tenant);
    await tenantDb.Database.MigrateAsync(ct);

    await SeedDefaultSportsAsync(tenantDb, ct);
    await SeedOwnerUserAsync(tenantDb, tenant, ct);
}
```

### Impacto

- **Elimina** 5+ classes mock duplicadas (`MigrationTenantContext`, `PublicTenantContext`, `StaticTenantContext`, `MigrationCurrentUserService`, `NullCurrentUserServiceInternal`)
- **Centraliza** toda a lógica de construção de `DbContextOptionsBuilder` em um lugar
- **Facilita** manutenção — se a configuração do contexto mudar, altera em um lugar só

---

## 6. Mover `CacheService` para Infrastructure

### Problema

O `CacheService` é uma implementação concreta que depende de `IDistributedCache` (Redis/StackExchange), mas está localizado no projeto `Application`:

```
src/SportHub.Application/Services/CacheService.cs   ← implementação concreta
```

Com namespace:

```csharp
namespace Infrastructure.Services;   // ← namespace de Infrastructure, mas arquivo está em Application!
```

Isso viola a Clean Architecture: **implementações concretas** com dependências de infraestrutura (Redis) devem ficar em **Infrastructure**. O projeto Application deve conter apenas interfaces e lógica de negócio.

### Solução

1. **Mover** o arquivo `src/SportHub.Application/Services/CacheService.cs` para `src/SportHub.Infrastructure/Services/CacheService.cs`
2. **Corrigir** o namespace (que já é `Infrastructure.Services`, então não muda)
3. **Garantir** que a interface `ICacheService` continue em `Application/Common/Interfaces/`
4. **Verificar** que o projeto `SportHub.Infrastructure` já tem referência ao pacote `Microsoft.Extensions.Caching.StackExchangeRedis` (deve ter)

A interface fica em Application:
```
src/SportHub.Application/Common/Interfaces/ICacheService.cs  ← interface (ok)
```

A implementação vai para Infrastructure:
```
src/SportHub.Infrastructure/Services/CacheService.cs  ← implementação (mover pra cá)
```

---

## 7. Converter endpoints sem MediatR em Queries

### Problema

Dois grupos de endpoints injetam repositórios diretamente, bypassando o pipeline MediatR:

1. **`AdminStatsEndpoints`** — injeta repositórios diretamente
2. **GET `/api/courts`** (listagem) — injeta `ICourtsRepository` diretamente

Isso significa que esses endpoints **não passam** pelo `ValidationBehavior` nem pelo `LoggingBehavior`. Além disso, quebram a consistência do padrão CQRS do projeto.

### Solução: Criar Queries para cada endpoint

**Exemplo para `AdminStatsEndpoints`:**

**Arquivo novo:** `src/SportHub.Application/UseCases/Admin/GetAdminStats/GetAdminStatsQuery.cs`

```csharp
using Application.CQRS;

namespace Application.UseCases.Admin.GetAdminStats;

public record GetAdminStatsQuery : IQuery<AdminStatsResponse>;

public record AdminStatsResponse(
    int TotalUsers,
    int TotalCourts,
    int TotalReservations,
    int TotalSports
);
```

**Arquivo novo:** `src/SportHub.Application/UseCases/Admin/GetAdminStats/GetAdminStatsHandler.cs`

```csharp
using Application.Common.Interfaces;
using Application.CQRS;
using FluentResults;

namespace Application.UseCases.Admin.GetAdminStats;

public class GetAdminStatsHandler : IQueryHandler<GetAdminStatsQuery, AdminStatsResponse>
{
    private readonly IUsersRepository _usersRepo;
    private readonly ICourtsRepository _courtsRepo;
    private readonly IReservationRepository _reservationRepo;
    private readonly ISportsRepository _sportsRepo;

    public GetAdminStatsHandler(
        IUsersRepository usersRepo,
        ICourtsRepository courtsRepo,
        IReservationRepository reservationRepo,
        ISportsRepository sportsRepo)
    {
        _usersRepo = usersRepo;
        _courtsRepo = courtsRepo;
        _reservationRepo = reservationRepo;
        _sportsRepo = sportsRepo;
    }

    public async Task<Result<AdminStatsResponse>> Handle(
        GetAdminStatsQuery request, CancellationToken ct)
    {
        var users = await _usersRepo.GetAllAsync();
        var courts = await _courtsRepo.GetAllAsync();
        var reservations = await _reservationRepo.GetAllAsync();
        var sports = await _sportsRepo.GetAllAsync();

        return Result.Ok(new AdminStatsResponse(
            users.Count,
            courts.Count,
            reservations.Count,
            sports.Count
        ));
    }
}
```

**Atualizar o endpoint para usar MediatR:**

```csharp
// ANTES:
app.MapGet("/admin/stats", async (
    IUsersRepository usersRepo,
    ICourtsRepository courtsRepo,
    // ... repos injetados diretamente
) => { /* lógica inline */ });

// DEPOIS:
app.MapGet("/admin/stats", async (ISender sender) =>
{
    var result = await sender.Send(new GetAdminStatsQuery());
    return result.ToIResult();
}).RequireAuthorization();
```

Fazer o mesmo para o GET `/api/courts` listagem — criar `GetAllCourtsQuery` + `GetAllCourtsHandler`.

---

## 8. SQL Injection no Interceptor de Schema

### Problema

Se o item 1 (migrar para `HasDefaultSchema`) for implementado, este problema é **automaticamente resolvido** porque o interceptor é eliminado.

Porém, se por algum motivo o interceptor for mantido, o SQL injection precisa ser corrigido:

**Código vulnerável:**

```csharp
command.CommandText = $"SET search_path TO \"{schema}\", public;";
```

O `schema` vem do header `X-Tenant-Slug` sem sanitização. Um atacante pode enviar:

```
X-Tenant-Slug: "; DROP TABLE "Users"; --
```

### Solução (se manter o interceptor)

Adicionar validação de slug via regex **antes** de usar o valor:

```csharp
using System.Text.RegularExpressions;

private static readonly Regex SafeSlugRegex = new(
    @"^[a-z0-9][a-z0-9\-]{0,61}[a-z0-9]$",
    RegexOptions.Compiled);

private string? GetCurrentSchema()
{
    // ... (extração de slug existente) ...

    if (string.IsNullOrWhiteSpace(slug)) return null;

    // ✅ Validação: rejeita slugs que não sejam alfanuméricos com hífens
    if (!SafeSlugRegex.IsMatch(slug))
    {
        _logger.LogWarning("Slug inválido rejeitado: {Slug}", slug);
        return null;
    }

    var schema = $"tenant_{slug.Replace("-", "_")}";
    return schema;
}
```

**Mas a recomendação principal continua sendo eliminar o interceptor** (item 1).

---

## 9. Usar `TimeProvider` em vez de `DateTime.UtcNow`

### Problema

A entidade `AuditEntity` usa `DateTime.UtcNow` diretamente, o que não é testável:

**Arquivo atual:** `src/SportHub.Domain/Common/AuditEntity.cs`

```csharp
public abstract class AuditEntity
{
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    public void SetCreated(Guid userId)
    {
        CreatedBy = userId;
        UpdatedBy = userId;
        CreatedAt = DateTime.UtcNow;  // ← não testável
        UpdatedAt = CreatedAt;
    }

    public void SetUpdated(Guid userId)
    {
        UpdatedBy = userId;
        UpdatedAt = DateTime.UtcNow;  // ← não testável
    }

    public void MarkAsDeleted(Guid userId)
    {
        IsDeleted = true;
        DeletedBy = userId;
        DeletedAt = DateTime.UtcNow;  // ← não testável
    }
}
```

Em testes unitários, é impossível controlar o "agora" para validar que datas foram preenchidas corretamente.

### Solução: Usar `TimeProvider` (.NET 8+)

O .NET 8 introduziu `TimeProvider` como abstração oficial para o tempo. Em `.NET 10` que vocês usam, está disponível nativamente.

**Opção A — Injetar `TimeProvider` via método (mínimo invasivo para Domain)**

Como o Domain não deve ter DI, a abordagem mais clean é receber o timestamp como parâmetro:

```csharp
// CÓDIGO NOVO
namespace SportHub.Domain.Common;

public abstract class AuditEntity
{
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public Guid CreatedBy { get; private set; }
    public Guid UpdatedBy { get; private set; }

    public bool IsDeleted { get; private set; } = false;
    public Guid? DeletedBy { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public void SetCreated(Guid userId, DateTime? utcNow = null)
    {
        var now = utcNow ?? DateTime.UtcNow;
        CreatedBy = userId;
        UpdatedBy = userId;
        CreatedAt = now;
        UpdatedAt = now;
    }

    public void SetUpdated(Guid userId, DateTime? utcNow = null)
    {
        var now = utcNow ?? DateTime.UtcNow;
        UpdatedBy = userId;
        UpdatedAt = now;
    }

    public void MarkAsDeleted(Guid userId, DateTime? utcNow = null)
    {
        var now = utcNow ?? DateTime.UtcNow;
        IsDeleted = true;
        DeletedBy = userId;
        DeletedAt = now;
    }

    public void Restore()
    {
        IsDeleted = false;
        DeletedBy = null;
        DeletedAt = null;
    }
}
```

**Opção B — Usar `TimeProvider` no `SaveChangesAsync` (melhor para centralizar)**

Se preferir que o `ApplicationDbContext` controle as datas (já que ele intercepta `SaveChangesAsync`):

```csharp
public class ApplicationDbContext : DbContext, IUnitOfWork
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ITenantContext _tenantContext;
    private readonly TimeProvider _timeProvider;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService currentUserService,
        ITenantContext tenantContext,
        TimeProvider timeProvider)  // ← novo parâmetro
        : base(options)
    {
        _currentUserService = currentUserService;
        _tenantContext = tenantContext;
        _timeProvider = timeProvider;
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        foreach (var entry in ChangeTracker.Entries<AuditEntity>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.SetCreated(userId, utcNow);
            else if (entry.State == EntityState.Modified)
                entry.Entity.SetUpdated(userId, utcNow);
            else if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.MarkAsDeleted(userId, utcNow);
                // ... (resto do soft delete)
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
```

**Registro no DI:**

```csharp
// Em produção usa o relógio real:
builder.Services.AddSingleton(TimeProvider.System);

// Em testes pode usar FakeTimeProvider:
// services.AddSingleton<TimeProvider>(new FakeTimeProvider(startDateTime));
```

---

## 10. Usar `ExpiryMinutes` do `JwtSettings`

### Problema

O `JwtSettings` tem uma propriedade `ExpiryMinutes` configurável via `appsettings.json`, mas o `JwtService` ignora completamente e usa `AddHours(2)` hardcoded:

```csharp
// Código provável no JwtService:
var expires = DateTime.UtcNow.AddHours(2); // ← ignora settings!
```

### Solução

```csharp
// CÓDIGO NOVO no JwtService
public string GenerateToken(User user)
{
    var expiryMinutes = _jwtSettings.ExpiryMinutes > 0
        ? _jwtSettings.ExpiryMinutes
        : 120; // fallback: 2h se não configurado

    var expires = DateTime.UtcNow.AddMinutes(expiryMinutes);

    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Expires = expires,
        // ... resto da configuração
    };

    // ...
}
```

**Garantir que o `appsettings.json` tenha o valor:**

```json
{
  "Jwt": {
    "Key": "...",
    "Issuer": "...",
    "Audience": "...",
    "ExpiryMinutes": 120
  }
}
```

---

## Ordem de Implementação Recomendada

Execute as mudanças nesta ordem para minimizar conflitos:

| Etapa | Item | Dependências |
|---|---|---|
| 1 | **Global Query Filter** (item 3) | Nenhuma — mudança isolada |
| 2 | **Unit of Work** (item 4) | Nenhuma — mas afeta todos os handlers |
| 3 | **`HasDefaultSchema` + `IModelCacheKeyFactory`** (itens 1 e 2) | Juntos — um depende do outro |
| 4 | **`ApplicationDbContextFactory`** (item 5) | Após item 1 (para simplificar a factory) |
| 5 | **Mover `CacheService`** (item 6) | Nenhuma — refactoring de organização |
| 6 | **Endpoints sem MediatR** (item 7) | Nenhuma — refactoring de padrão |
| 7 | **`TimeProvider`** (item 9) | Nenhuma — mudança isolada |
| 8 | **`ExpiryMinutes`** (item 10) | Nenhuma — fix trivial |

> **Dica:** Faça um commit por item. Se algo der errado, é fácil reverter.

---

## Checklist Final

- [ ] Migrar `ApplicationDbContext` para usar `HasDefaultSchema`
- [ ] Atualizar `TenantModelCacheKeyFactory` para incluir schema na chave
- [ ] Deletar `TenantSchemaConnectionInterceptor`
- [ ] Remover registro do interceptor no DI (`ServiceExtensions.AddDatabase`)
- [ ] Adicionar Global Query Filter para soft delete no `OnModelCreating`
- [ ] Adicionar import `System.Linq.Expressions` no `AppDbContext.cs`
- [ ] Criar interface `IUnitOfWork` em `Application/Common/Interfaces/`
- [ ] Implementar `IUnitOfWork` no `ApplicationDbContext`
- [ ] Remover `SaveChangesAsync` de todos os métodos do `BaseRepository`
- [ ] Registrar `IUnitOfWork` no DI
- [ ] Atualizar **todos os handlers de Command** para injetar `IUnitOfWork`
- [ ] Criar `ApplicationDbContextFactory` em Infrastructure
- [ ] Refatorar `ExecuteMigrations` para usar a factory
- [ ] Refatorar `TenantProvisioningService` para usar a factory
- [ ] Deletar classes mock (`MigrationTenantContext`, `PublicTenantContext`, etc.)
- [ ] Mover `CacheService.cs` de Application para Infrastructure
- [ ] Criar `GetAdminStatsQuery` + Handler
- [ ] Criar `GetAllCourtsQuery` + Handler
- [ ] Atualizar endpoints para usar MediatR
- [ ] Adicionar `TimeProvider` ao `SaveChangesAsync` (Opção B)
- [ ] Registrar `TimeProvider.System` no DI
- [ ] Corrigir `JwtService` para usar `ExpiryMinutes` do settings
- [ ] Re-rodar migrations para validar que tudo funciona com `HasDefaultSchema`
- [ ] Rodar testes existentes
