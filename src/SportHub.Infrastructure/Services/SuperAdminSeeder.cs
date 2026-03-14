using Application.Common.Interfaces;
using Application.Settings;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

/// <summary>
/// Cria o usuário SuperAdmin no schema "public".
/// Usa ApplicationDbContext com PublicSchemaTenantContext apontando para o schema "public".
/// Executa apenas no startup, antes de qualquer request.
/// </summary>
public class SuperAdminSeeder
{
    private readonly TenantDbContext _tenantDbContext;
    private readonly IPasswordService _passwordService;
    private readonly AdminUserSettings _adminSettings;

    public SuperAdminSeeder(
        TenantDbContext tenantDbContext,
        IPasswordService passwordService,
        IOptions<AdminUserSettings> adminSettings)
    {
        _tenantDbContext = tenantDbContext;
        _passwordService = passwordService;
        _adminSettings = adminSettings.Value;
    }

    public async Task SeedAsync()
    {
        var connectionString = _tenantDbContext.Database.GetConnectionString()!;

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(connectionString);
        optionsBuilder.ReplaceService<Microsoft.EntityFrameworkCore.Infrastructure.IModelCacheKeyFactory,
            TenantModelCacheKeyFactory>();
        optionsBuilder.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));

        await using var db = new ApplicationDbContext(
            optionsBuilder.Options,
            new SuperAdminNullUserService(),
            new PublicSchemaTenantContext(),
            TimeProvider.System
        );

        var exists = await db.Users.AnyAsync(u => u.Email == _adminSettings.Email);
        if (exists)
            return;

        var passwordHash = _passwordService.HashPassword(_adminSettings.Password, out var salt);

        var superAdmin = new User
        {
            Id = Guid.NewGuid(),
            Email = _adminSettings.Email,
            FirstName = _adminSettings.FirstName,
            LastName = _adminSettings.LastName,
            PasswordHash = passwordHash,
            Salt = salt,
            Role = UserRole.SuperAdmin,
            IsActive = true,
        };

        db.Users.Add(superAdmin);
        await db.SaveChangesAsync();
    }
}

internal class PublicSchemaTenantContext : ITenantContext
{
    public Guid TenantId => Guid.Empty;
    public string TenantSlug => "system";
    public string TenantName => "System";
    public string? LogoUrl => null;
    public string? PrimaryColor => null;
    public string Schema => "public";
    public bool IsResolved => true;
    public void Resolve(Tenant tenant) { }
}

internal class SuperAdminNullUserService : ICurrentUserService
{
    public Guid UserId => Guid.Empty;
}
