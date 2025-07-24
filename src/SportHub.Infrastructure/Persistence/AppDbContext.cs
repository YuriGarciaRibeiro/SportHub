using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Establishment> Establishments { get; set; } = null!;
    public DbSet<EstablishmentUser> EstablishmentUsers { get; set; } = null!;
    public DbSet<Domain.Entities.Court> Courts { get; set; } = null!;

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
    }

}
