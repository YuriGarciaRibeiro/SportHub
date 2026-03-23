using Application.Common.Interfaces;
using Application.UseCases.Tenant.ProvisionTenant;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SportHub.Domain.Common;

namespace Infrastructure.Services;

public class TenantProvisioningService : ITenantProvisioningService
{
    private readonly ApplicationDbContext _db;
    private readonly ITenantRepository _tenantRepository;
    private readonly IPasswordService _passwordService;
    private readonly ILogger<TenantProvisioningService> _logger;

    public TenantProvisioningService(
        ApplicationDbContext db,
        ITenantRepository tenantRepository,
        IPasswordService passwordService,
        ILogger<TenantProvisioningService> logger)
    {
        _db = db;
        _tenantRepository = tenantRepository;
        _passwordService = passwordService;
        _logger = logger;
    }

    public async Task ProvisionAsync(Tenant tenant, ProvisionTenantCommand command, CancellationToken ct = default)
    {
        _logger.LogInformation("Iniciando provisioning do tenant {Slug}", tenant.Slug);

        await _tenantRepository.AddAsync(tenant, ct);
        _logger.LogInformation("Tenant {Slug} salvo", tenant.Slug);

        await SeedDefaultSportsAsync(tenant.Id, ct);
        await SeedOwnerUserAsync(tenant, ct);
        await SeedDefaultLocationAsync(tenant, command, ct);

        _logger.LogInformation("Provisioning do tenant {Slug} concluído", tenant.Slug);
    }

    private async Task SeedDefaultSportsAsync(Guid tenantId, CancellationToken ct)
    {
        var defaultSports = new[]
        {
            new Sport { Id = Guid.NewGuid(), TenantId = tenantId, Name = "Futebol Society", Description = "Futebol em campo society (7 a side)", ImageUrl = "" },
            new Sport { Id = Guid.NewGuid(), TenantId = tenantId, Name = "Beach Tennis", Description = "Beach Tennis em quadra de areia", ImageUrl = "" },
            new Sport { Id = Guid.NewGuid(), TenantId = tenantId, Name = "Padel", Description = "Padel em quadra fechada", ImageUrl = "" },
            new Sport { Id = Guid.NewGuid(), TenantId = tenantId, Name = "Tênis", Description = "Tênis em quadra de saibro ou dura", ImageUrl = "" },
            new Sport { Id = Guid.NewGuid(), TenantId = tenantId, Name = "Vôlei de Praia", Description = "Vôlei em quadra de areia", ImageUrl = "" },
            new Sport { Id = Guid.NewGuid(), TenantId = tenantId, Name = "Basquete", Description = "Basquete em quadra coberta", ImageUrl = "" },
        };

        _db.Sports.AddRange(defaultSports);
        await _db.SaveChangesAsync(ct);
    }

    private async Task SeedOwnerUserAsync(Tenant tenant, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(tenant.OwnerEmail))
        {
            _logger.LogWarning("OwnerEmail não fornecido. O usuário dono não será criado automaticamente.");
            return;
        }

        var exists = await _db.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => u.TenantId == tenant.Id && u.Email == tenant.OwnerEmail, ct);
        if (exists) return;

        string defaultPassword = "Owner@123";
        var passwordHash = _passwordService.HashPassword(defaultPassword, out var salt);

        var ownerUser = new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            Email = tenant.OwnerEmail,
            FirstName = tenant.OwnerFirstName ?? "Owner",
            LastName = tenant.OwnerLastName ?? "Tenant",
            PasswordHash = passwordHash,
            Salt = salt,
            Role = Domain.Enums.UserRole.Owner,
            IsActive = true
        };
        ownerUser.SetCreated(Guid.Empty);

        _db.Users.Add(ownerUser);
        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Usuário Owner {Email} criado para o tenant {Slug}", tenant.OwnerEmail, tenant.Slug);
    }

    private async Task SeedDefaultLocationAsync(Tenant tenant, ProvisionTenantCommand command, CancellationToken ct)
    {
        var exists = await _db.Locations
            .IgnoreQueryFilters()
            .AnyAsync(l => l.TenantId == tenant.Id, ct);
        if (exists) return;

        Address? address = null;
        if (command.Address is not null)
        {
            address = new Address
            {
                Street = command.Address.Street,
                Number = command.Address.Number,
                Complement = command.Address.Complement,
                Neighborhood = command.Address.Neighborhood,
                City = command.Address.City,
                State = command.Address.State,
                ZipCode = command.Address.ZipCode
            };
        }

        var location = new Location
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            Name = command.LocationName ?? $"{tenant.Name} — Sede Principal",
            IsDefault = true,
            Address = address,
            Phone = command.Phone,
            BusinessHours = []
        };
        location.SetCreated(Guid.Empty);

        _db.Locations.Add(location);
        await _db.SaveChangesAsync(ct);
    }

    public async Task ProvisionOwnerUserAsync(Tenant tenant, CancellationToken ct = default)
    {
        await SeedOwnerUserAsync(tenant, ct);
    }
}
