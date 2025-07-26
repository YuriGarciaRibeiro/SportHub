using Application.Common.Interfaces;
using Application.Settings;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public class CustomUserSeeder
{
    private readonly IUsersRepository _usersRepository;
    private readonly IPasswordService _passwordService;
    private readonly AdminUserSettings _adminSettings;

    public CustomUserSeeder(
        IUsersRepository usersRepository,
        IPasswordService passwordService,
        IOptions<AdminUserSettings> adminSettings)
    {
        _usersRepository = usersRepository;
        _passwordService = passwordService;
        _adminSettings = adminSettings.Value;
    }

    public async Task SeedAsync()
    {
        var existingAdmin = await _usersRepository.GetByEmailAsync(_adminSettings.Email);
        if (existingAdmin != null)
            return;

        var passwordHash = _passwordService.HashPassword(_adminSettings.Password, out var salt);

        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            Email = _adminSettings.Email,
            FirstName = _adminSettings.FirstName,
            LastName = _adminSettings.LastName,
            PasswordHash = passwordHash,
            Salt = salt,
            Role = UserRole.Admin,
            IsActive = true,
        };

        await _usersRepository.CreateAsync(adminUser);
    }
}
