using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class CourtSportConfiguration : IEntityTypeConfiguration<Court>
{
    public void Configure(EntityTypeBuilder<Court> builder)
    {
        builder.HasMany(c => c.Sports)
            .WithMany(s => s.Courts)
            .UsingEntity<Dictionary<string, object>>(
                "CourtSport",
                j => j.HasOne<Sport>()
                    .WithMany()
                    .HasForeignKey("SportsId")
                    .OnDelete(DeleteBehavior.Cascade),
                j => j.HasOne<Court>()
                    .WithMany()
                    .HasForeignKey("CourtsId")
                    .OnDelete(DeleteBehavior.Cascade),
                j =>
                {
                    j.HasKey("CourtsId", "SportsId");
                    j.HasIndex("CourtsId", "SportsId").IsUnique();
                    j.ToTable("CourtSport");
                });
    }
}
