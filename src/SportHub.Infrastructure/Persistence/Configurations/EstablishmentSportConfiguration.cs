using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class EstablishmentSportConfiguration : IEntityTypeConfiguration<Establishment>
{
    public void Configure(EntityTypeBuilder<Establishment> builder)
    {
        builder.HasMany(e => e.Sports)
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
                    j.HasIndex("EstablishmentId", "SportId").IsUnique();
                    j.ToTable("EstablishmentSports");
                });
    }
}