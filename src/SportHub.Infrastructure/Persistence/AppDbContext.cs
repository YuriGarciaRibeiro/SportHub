using Application.Common.Interfaces;
using Domain.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Email).IsRequired().HasMaxLength(256);
            entity.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.LastName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.Salt).IsRequired();
            entity.Property(u => u.Role).HasConversion<string>();
        });

        builder.Entity<EstablishmentUser>()
            .HasKey(x => new { x.UserId, x.EstablishmentId });

        builder.Entity<EstablishmentUser>()
            .HasOne<User>()
            .WithMany(u => u.Establishments)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<EstablishmentUser>()
            .HasOne(x => x.Establishment)
            .WithMany(e => e.Users)
            .HasForeignKey(x => x.EstablishmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<EstablishmentUser>()
            .Property(x => x.Role)
            .HasConversion<string>();

        builder.Entity<Establishment>(entity =>
        {
            entity.OwnsOne(e => e.Address);
        });

        builder.Entity<Establishment>()
            .HasMany(e => e.Sports)
            .WithMany(s => s.Establishments)
            .UsingEntity<Dictionary<string, object>>(
                "EstablishmentSport",
                j => j.HasOne<Sport>()
                    .WithMany()
                    .HasForeignKey("SportId")
                    .OnDelete(DeleteBehavior.Cascade),
                j => j.HasOne<Establishment>()
                    .WithMany()
                    .HasForeignKey("EstablishmentId")
                    .OnDelete(DeleteBehavior.Cascade),
                j =>
                {
                    j.HasKey("EstablishmentId", "SportId");
                    j.HasIndex(new[] { "EstablishmentId", "SportId" }).IsUnique();
                    j.ToTable("EstablishmentSports");
                });
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
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }

}
