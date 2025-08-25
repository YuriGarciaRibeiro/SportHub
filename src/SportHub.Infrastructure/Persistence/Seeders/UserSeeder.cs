using Application.Settings;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Persistence.Seeders;

public class UserSeeder : BaseSeeder
{
    private readonly IUsersRepository _usersRepository;
    private readonly IPasswordService _passwordService;
    private readonly AdminUserSettings _adminSettings;

    public UserSeeder(
        IUsersRepository usersRepository,
        IPasswordService passwordService,
        IOptions<AdminUserSettings> adminSettings,
        ILogger<UserSeeder> logger) : base(logger)
    {
        _usersRepository = usersRepository;
        _passwordService = passwordService;
        _adminSettings = adminSettings.Value;
    }

    public override int Order => 1; // Primeiro a ser executado

    public override async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        LogInfo("Starting user seeding...");

        await SeedAdminUserAsync(cancellationToken);
        await SeedTestUsersAsync(cancellationToken);

        LogInfo("User seeding completed.");
    }

    private async Task SeedAdminUserAsync(CancellationToken cancellationToken)
    {
        var existingAdmin = await _usersRepository.GetByEmailAsync(_adminSettings.Email, cancellationToken);
        if (existingAdmin != null)
        {
            LogInfo($"Admin user already exists: {_adminSettings.Email}");
            return;
        }

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

        await _usersRepository.AddAsync(adminUser, cancellationToken);
        LogInfo($"Created admin user: {_adminSettings.Email}");
    }

    private async Task SeedTestUsersAsync(CancellationToken cancellationToken)
    {
        var testUsers = GetTestUsers();

        foreach (var userData in testUsers)
        {
            var existing = await _usersRepository.GetByEmailAsync(userData.Email, cancellationToken);
            if (existing != null)
            {
                LogInfo($"Test user already exists: {userData.Email}");
                continue;
            }

            var passwordHash = _passwordService.HashPassword("SportHub123!", out var salt);

            var user = new User
            {
                Id = userData.Id,
                Email = userData.Email,
                FirstName = userData.FirstName,
                LastName = userData.LastName,
                PasswordHash = passwordHash,
                Salt = salt,
                Role = userData.Role,
                IsActive = true,
            };

            await _usersRepository.AddAsync(user, cancellationToken);
            LogInfo($"Created test user: {userData.Email}");
        }
    }

    private List<(Guid Id, string Email, string FirstName, string LastName, UserRole Role)> GetTestUsers()
    {
        return new List<(Guid, string, string, string, UserRole)>
        {
            // Establishment owners
            (Guid.Parse("11111111-1111-1111-1111-111111111111"), "john.smith@sporthub.com", "John", "Smith", UserRole.EstablishmentMember),
            (Guid.Parse("22222222-2222-2222-2222-222222222222"), "mary.johnson@sporthub.com", "Mary", "Johnson", UserRole.EstablishmentMember),
            (Guid.Parse("33333333-3333-3333-3333-333333333333"), "charles.williams@sporthub.com", "Charles", "Williams", UserRole.EstablishmentMember),
            
            // Regular users
            (Guid.Parse("44444444-4444-4444-4444-444444444444"), "anna.brown@email.com", "Anna", "Brown", UserRole.User),
            (Guid.Parse("55555555-5555-5555-5555-555555555555"), "peter.davis@email.com", "Peter", "Davis", UserRole.User),
            (Guid.Parse("66666666-6666-6666-6666-666666666666"), "lucy.miller@email.com", "Lucy", "Miller", UserRole.User),
            (Guid.Parse("77777777-7777-7777-7777-777777777777"), "robert.wilson@email.com", "Robert", "Wilson", UserRole.User),
            (Guid.Parse("88888888-8888-8888-8888-888888888888"), "emily.moore@email.com", "Emily", "Moore", UserRole.User),
        };
    }
}
