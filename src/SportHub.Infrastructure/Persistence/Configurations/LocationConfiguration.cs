using System.Text.Json;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportHub.Domain.Common;

namespace Infrastructure.Persistence.Configurations;

public class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.OwnsOne(l => l.Address, address =>
        {
            address.Property(a => a.Street).HasMaxLength(200).HasColumnName("AddressStreet");
            address.Property(a => a.Number).HasMaxLength(20).HasColumnName("AddressNumber");
            address.Property(a => a.Complement).HasMaxLength(100).HasColumnName("AddressComplement");
            address.Property(a => a.Neighborhood).HasMaxLength(100).HasColumnName("AddressNeighborhood");
            address.Property(a => a.City).HasMaxLength(100).HasColumnName("AddressCity");
            address.Property(a => a.State).HasMaxLength(2).HasColumnName("AddressState");
            address.Property(a => a.ZipCode).HasMaxLength(10).HasColumnName("AddressZipCode");
        });

        builder.Property(l => l.Phone)
            .HasMaxLength(30);

        builder.Property(l => l.BusinessHours)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<List<DailySchedule>>(v, JsonOptions) ?? new List<DailySchedule>());

        builder.Property(l => l.InstagramUrl)
            .HasMaxLength(500);

        builder.Property(l => l.FacebookUrl)
            .HasMaxLength(500);

        builder.Property(l => l.WhatsappNumber)
            .HasMaxLength(20);

        builder.HasIndex(l => l.TenantId);
        builder.HasIndex(l => l.IsDefault);

        builder.HasMany(l => l.Courts)
            .WithOne(c => c.Location)
            .HasForeignKey(c => c.LocationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
