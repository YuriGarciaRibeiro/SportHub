using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants", "public");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Slug)
            .IsRequired()
            .HasMaxLength(63);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(t => t.LogoUrl)
            .HasMaxLength(500);

        builder.Property(t => t.CoverImageUrl)
            .HasMaxLength(500);

        builder.Property(t => t.PrimaryColor)
            .HasMaxLength(7);

        builder.Property(t => t.CustomDomain)
            .HasMaxLength(253);

        builder.Property(t => t.InstagramUrl)
            .HasMaxLength(500);

        builder.Property(t => t.FacebookUrl)
            .HasMaxLength(500);

        builder.Property(t => t.WhatsappNumber)
            .HasMaxLength(20);

        builder.HasIndex(t => t.Slug).IsUnique();
        builder.HasIndex(t => t.CustomDomain).IsUnique();
        builder.HasIndex(t => t.Status);
    }
}
