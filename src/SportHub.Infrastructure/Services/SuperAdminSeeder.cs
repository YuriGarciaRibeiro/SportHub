using Application.Settings;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Application.Common.Interfaces;

namespace Infrastructure.Services;

public class SuperAdminSeeder
{
    private readonly ApplicationDbContext _db;
    private readonly IPasswordService _passwordService;
    private readonly AdminUserSettings _adminSettings;

    public SuperAdminSeeder(
        ApplicationDbContext db,
        IPasswordService passwordService,
        IOptions<AdminUserSettings> adminSettings)
    {
        _db = db;
        _passwordService = passwordService;
        _adminSettings = adminSettings.Value;
    }

    public async Task SeedAsync()
    {
        var exists = await _db.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Email == _adminSettings.Email);
        if (exists) return;

        var passwordHash = _passwordService.HashPassword(_adminSettings.Password, out var salt);

        var superAdmin = new User
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.Empty,
            Email = _adminSettings.Email,
            FirstName = _adminSettings.FirstName,
            LastName = _adminSettings.LastName,
            PasswordHash = passwordHash,
            Salt = salt,
            Role = UserRole.SuperAdmin,
            IsActive = true,
        };

        _db.Users.Add(superAdmin);
        await _db.SaveChangesAsync();
    }
}
