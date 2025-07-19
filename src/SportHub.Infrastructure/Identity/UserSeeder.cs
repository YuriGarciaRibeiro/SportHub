using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Domain.Enums;
using Application.Settings;

namespace Infrastructure.Identity;

public class UserSeeder
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly AdminUserSettings _adminSettings;
    private readonly ILogger<UserSeeder> _logger;

    public UserSeeder(
        UserManager<AppUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        IOptions<AdminUserSettings> adminOptions,
        ILogger<UserSeeder> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _adminSettings = adminOptions.Value;
        _logger = logger;
    }

    public async Task SeedAdminAsync()
    {
        var email = _adminSettings.Email;
        var password = _adminSettings.Password;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            _logger.LogWarning("Admin seed skipped: missing Email or Password.");
            return;
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user != null)
        {
            _logger.LogInformation("Admin user already exists: {Email}", email);
            return;
        }

        var admin = new AppUser
        {
            Id = Guid.NewGuid(),
            Email = email,
            UserName = email,
            FirstName = "System",
            LastName = "Admin",
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(admin, password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("Failed to seed admin user: {Errors}", errors);
            throw new Exception($"Failed to seed admin: {errors}");
        }

        await _userManager.AddToRoleAsync(admin, UserRole.Admin.ToString());

        _logger.LogInformation("Admin user seeded successfully: {Email}", email);
    }
}
