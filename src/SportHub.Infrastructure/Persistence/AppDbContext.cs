using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using SportHub.Domain.Common;
using System.Linq.Expressions;

namespace Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IUnitOfWork
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ITenantContext _tenantContext;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Schema atual — exposto para TenantModelCacheKeyFactory gerar chave de cache correta.
    /// </summary>
    public string CurrentSchema => _tenantContext.Schema;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService currentUserService,
        ITenantContext tenantContext,
        TimeProvider timeProvider)
        : base(options)
    {
        _currentUserService = currentUserService;
        _tenantContext = tenantContext;
        _timeProvider = timeProvider;
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

        // Global Query Filter para soft delete.
        // Aplica automaticamente WHERE "IsDeleted" = false em TODAS as queries
        // de entidades que herdam AuditEntity.
        ApplySoftDeleteFilter(builder);
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
}
