using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure.Persistence;

/// <summary>
/// Usado APENAS pelo EF CLI (dotnet ef migrations add).
/// Nunca é instanciado em produção.
/// Retorna um ApplicationDbContext com schema "tenant_placeholder" para geração de migrations.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=sporthub;Username=postgres;Password=postgres");

        return new ApplicationDbContext(
            optionsBuilder.Options,
            new NullCurrentUserService(),
            new PlaceholderTenantContext(),
            TimeProvider.System
        );
    }
}

internal class NullCurrentUserService : ICurrentUserService
{
    public Guid UserId => Guid.Empty;
}

internal class PlaceholderTenantContext : ITenantContext
{
    public Guid TenantId => Guid.Empty;
    public string TenantSlug => "placeholder";
    public string TenantName => "Placeholder";
    public string? LogoUrl => null;
    public string? PrimaryColor => null;
    public string Schema => "tenant_placeholder";
    public bool IsResolved => true;
    public void Resolve(Tenant tenant) { }
}
