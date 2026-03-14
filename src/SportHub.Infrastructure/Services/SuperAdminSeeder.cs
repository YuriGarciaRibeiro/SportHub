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
/// Usa ApplicationDbContextFactory.CreateForPublic() para obter o contexto.
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
        var factory = new ApplicationDbContextFactory(connectionString);

        await using var db = factory.CreateForPublic();

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
