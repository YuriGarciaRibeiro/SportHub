using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.HasKey(r => r.Id);

        builder.HasIndex(r => r.CourtId);
        builder.HasIndex(r => r.UserId);
        builder.HasIndex(r => new { r.CourtId, r.StartTimeUtc });
    }
}
