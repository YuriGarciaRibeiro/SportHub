using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class EstablishmentUserConfiguration : IEntityTypeConfiguration<EstablishmentUser>
{
    public void Configure(EntityTypeBuilder<EstablishmentUser> builder)
    {
        builder.HasKey(x => new { x.UserId, x.EstablishmentId });

        builder.HasOne<User>()
            .WithMany(u => u.Establishments)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Establishment)
            .WithMany(e => e.Users)
            .HasForeignKey(x => x.EstablishmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.Role)
            .HasConversion<string>();

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.EstablishmentId);
        builder.HasIndex(x => x.Role);
    }
}
