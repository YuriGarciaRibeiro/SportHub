using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class EstablishmentConfiguration : IEntityTypeConfiguration<Establishment>
{
    public void Configure(EntityTypeBuilder<Establishment> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);

        builder.HasIndex(e => e.Name);

        builder.OwnsOne(e => e.Address, a =>
        {
            a.Property(x => x.City).HasMaxLength(100);
            a.Property(x => x.State).HasMaxLength(100);
            
            a.Property(x => x.Location)
             .HasColumnType("geography (point)");
            
            a.HasIndex(x => x.City);
            a.HasIndex(x => x.State);
            
            a.HasIndex(x => x.Location)
             .HasMethod("GIST");
        });
    }
}
