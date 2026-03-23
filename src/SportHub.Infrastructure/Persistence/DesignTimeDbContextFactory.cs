using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Persistence;

/// <summary>
/// Usado exclusivamente pelo EF CLI (dotnet ef migrations add/update).
/// Lê a connection string do appsettings.Development.json ou variável de ambiente.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../SportHub.Api"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsAssembly("SportHub.Infrastructure"))
            .Options;

        return new ApplicationDbContext(options, new NullCurrentUserService(), new NullTenantContext(), TimeProvider.System);
    }

    private sealed class NullCurrentUserService : ICurrentUserService
    {
        public Guid UserId => Guid.Empty;
        public Domain.Enums.UserRole? UserRole => null;
    }

    private sealed class NullTenantContext : ITenantContext
    {
        public Guid TenantId => Guid.Empty;
        public string TenantSlug => string.Empty;
        public string TenantName => string.Empty;
        public string? LogoUrl => null;
        public string? CoverImageUrl => null;
        public string? PrimaryColor => null;
        public string? Tagline => null;
        public string? InstagramUrl => null;
        public string? FacebookUrl => null;
        public string? WhatsappNumber => null;
        public bool IsResolved => false;
        public void Resolve(Tenant tenant) { }
    }
}
