using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.HasKey(r => r.Id);

        builder.HasIndex(r => r.TenantId);
        builder.HasIndex(r => r.CourtId);
        builder.HasIndex(r => r.UserId);
        builder.HasIndex(r => new { r.CourtId, r.StartTimeUtc });

        builder.Property(r => r.Status)
               .HasConversion<int>()
               .HasDefaultValue(ReservationStatus.Pending);

        builder.HasOne(r => r.CreatedByUser)
               .WithMany()
               .HasForeignKey("CreatedBy")
               .OnDelete(DeleteBehavior.Restrict);
    }
}
