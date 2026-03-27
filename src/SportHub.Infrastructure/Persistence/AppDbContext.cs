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

    // Global (shared across tenants)
    public DbSet<Tenant> Tenants { get; set; } = null!;

    // Tenant-scoped
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Court> Courts { get; set; } = null!;
    public DbSet<Sport> Sports { get; set; } = null!;
    public DbSet<Reservation> Reservations { get; set; } = null!;
    public DbSet<Location> Locations { get; set; } = null!;

    private Guid CurrentTenantId => _tenantContext.TenantId;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        ApplyGlobalFilters(builder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        foreach (var entry in ChangeTracker.Entries<TenantEntity>())
        {
            if (entry.State == EntityState.Added && _tenantContext.IsResolved)
                entry.Entity.TenantId = _tenantContext.TenantId;
        }

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

    private void ApplyGlobalFilters(ModelBuilder builder)
    {
        var dbContextConst = Expression.Constant(this, typeof(ApplicationDbContext));
        var currentTenantIdExpr = Expression.Property(dbContextConst, nameof(CurrentTenantId));

        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;
            var isTenant = typeof(TenantEntity).IsAssignableFrom(clrType);
            var isAudit = typeof(AuditEntity).IsAssignableFrom(clrType);

            if (!isAudit) continue;

            var param = Expression.Parameter(clrType, "e");

            // !e.IsDeleted
            var isDeletedProp = Expression.Property(param, nameof(AuditEntity.IsDeleted));
            Expression body = Expression.Not(isDeletedProp);

            if (isTenant)
            {
                // e.TenantId == this.CurrentTenantId
                var tenantIdProp = Expression.Property(param, nameof(TenantEntity.TenantId));
                var tenantFilter = Expression.Equal(tenantIdProp, currentTenantIdExpr);
                body = Expression.AndAlso(body, tenantFilter);
            }

            builder.Entity(clrType).HasQueryFilter(Expression.Lambda(body, param));
        }
    }
}
