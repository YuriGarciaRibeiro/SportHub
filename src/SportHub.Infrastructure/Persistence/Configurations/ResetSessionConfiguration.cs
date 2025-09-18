using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ResetSessionConfiguration : IEntityTypeConfiguration<ResetSession>
{
    public void Configure(EntityTypeBuilder<ResetSession> builder)
    {
        builder.ToTable("ResetSessions");
    }
}