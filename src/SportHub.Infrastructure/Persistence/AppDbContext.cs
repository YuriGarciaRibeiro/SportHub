using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using SportHub.Domain.Common;
using System.Linq.Expressions;

namespace Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IUnitOfWork
{
    private readonly ICurrentUserService _currentUserService;
    private readonly TimeProvider _timeProvider;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService currentUserService,
        ITenantContext tenantContext,
        TimeProvider timeProvider)
        : base(options)
    {
        _currentUserService = currentUserService;
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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

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

    private static void ApplySoftDeleteFilter(ModelBuilder builder)
    {
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (!typeof(AuditEntity).IsAssignableFrom(entityType.ClrType))
                continue;

            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var isDeleted = Expression.Property(parameter, nameof(AuditEntity.IsDeleted));
            var filter = Expression.Lambda(Expression.Not(isDeleted), parameter);

            builder.Entity(entityType.ClrType).HasQueryFilter(filter);
        }
    }
}
