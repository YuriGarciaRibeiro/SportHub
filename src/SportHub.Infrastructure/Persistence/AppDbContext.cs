using Domain.Entities;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<Establishment> Establishments { get; set; } = null!;
    public DbSet<EstablishmentUser> EstablishmentUsers { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<EstablishmentUser>()
            .HasKey(x => new { x.UserId, x.EstablishmentId });

        builder.Entity<EstablishmentUser>()
            .HasOne<AppUser>() 
            .WithMany()
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
