using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class CourtConfiguration : IEntityTypeConfiguration<Court>
{
    public void Configure(EntityTypeBuilder<Court> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.PricePerHour)
            .HasPrecision(10, 2);

        builder.Property(c => c.ImageUrl)
            .HasMaxLength(512);

        builder.Property(c => c.Amenities)
            .HasColumnType("text[]");

        builder.Property(c => c.ImageUrls)
            .HasColumnType("text[]");

        builder.Property(c => c.LocationId)
            .IsRequired();

        builder.HasIndex(c => c.TenantId);
        builder.HasIndex(c => c.Name);
        builder.HasIndex(c => c.LocationId);
    }
}
