using Application.Common.Interfaces;
using Domain.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{

    private readonly ICurrentUserService _currentUserService;
    
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ICurrentUserService currentUserService)
        : base(options)
    {
        _currentUserService = currentUserService;
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Establishment> Establishments { get; set; } = null!;
    public DbSet<EstablishmentUser> EstablishmentUsers { get; set; } = null!;
    public DbSet<Court> Courts { get; set; } = null!;
    public DbSet<Sport> Sports { get; set; } = null!;
    public DbSet<Reservation> Reservations { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
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
