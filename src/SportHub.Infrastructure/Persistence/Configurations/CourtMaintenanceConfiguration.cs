using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class CourtMaintenanceConfiguration : IEntityTypeConfiguration<CourtMaintenance>
{
    public void Configure(EntityTypeBuilder<CourtMaintenance> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Description)
            .HasMaxLength(200);

        builder.HasIndex(m => m.CourtId);
        builder.HasIndex(m => new { m.CourtId, m.Date });

        builder
            .HasOne(m => m.Court)
            .WithMany(c => c.Maintenances)
            .HasForeignKey(m => m.CourtId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
