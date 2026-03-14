using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class TenantProvisioningService : ITenantProvisioningService
{
    private readonly TenantDbContext _globalCtx;
    private readonly IServiceProvider _serviceProvider;
    private readonly ITenantRepository _tenantRepository;
    private readonly IPasswordService _passwordService;
    private readonly ILogger<TenantProvisioningService> _logger;

    public TenantProvisioningService(
        TenantDbContext globalCtx,
        IServiceProvider serviceProvider,
        ITenantRepository tenantRepository,
        IPasswordService passwordService,
        ILogger<TenantProvisioningService> logger)
    {
        _globalCtx = globalCtx;
        _serviceProvider = serviceProvider;
        _tenantRepository = tenantRepository;
        _passwordService = passwordService;
        _logger = logger;
    }

    /// <summary>
    /// Provisiona um novo tenant:
    /// 1. Salva o Tenant na tabela global (schema public)
    /// 2. Cria o schema PostgreSQL: CREATE SCHEMA IF NOT EXISTS tenant_{slug}
    /// 3. Executa migrations do ApplicationDbContext nesse schema
    /// 4. Faz seed dos Sports padrão
    /// </summary>
    public async Task ProvisionAsync(Tenant tenant, CancellationToken ct = default)
    {
        _logger.LogInformation("Iniciando provisioning do tenant {Slug}", tenant.Slug);

        await _tenantRepository.AddAsync(tenant, ct);
        _logger.LogInformation("Tenant {Slug} salvo no schema public", tenant.Slug);

        var schemaName = tenant.GetSchemaName();
        await _globalCtx.Database.ExecuteSqlAsync(
            $"CREATE SCHEMA IF NOT EXISTS \"{schemaName}\"", ct);
        _logger.LogInformation("Schema {Schema} criado", schemaName);

        var connectionString = _globalCtx.Database.GetConnectionString()!;
        var factory = new ApplicationDbContextFactory(connectionString);

        await using var tenantDb = factory.CreateForTenant(tenant);

        try
        {
            await tenantDb.Database.MigrateAsync(ct);
            _logger.LogInformation("Migrations aplicadas no schema {Schema}", schemaName);
        }
        catch (Exception ex) when (ex.InnerException?.Message.Contains("42P07") == true
                                    || ex.Message.Contains("already exists"))
        {
            // Schema já tinha as tabelas (provisionado anteriormente ou migração parcial).
            // Apenas logamos e continuamos — o seed vai lidar com IdempotentAsync.
            _logger.LogWarning("Schema {Schema} já possuía tabelas. Pulando migration. Detalhe: {Msg}",
                schemaName, ex.InnerException?.Message ?? ex.Message);
        }

        await SeedDefaultSportsAsync(tenantDb, ct);
        await SeedOwnerUserAsync(tenantDb, tenant, ct);

        _logger.LogInformation("Provisioning do tenant {Slug} concluído", tenant.Slug);
    }

    private static async Task SeedDefaultSportsAsync(ApplicationDbContext db, CancellationToken ct)
    {
        var defaultSports = new[]
        {
            new Sport { Id = Guid.NewGuid(), Name = "Futebol Society", Description = "Futebol em campo society (7 a side)", ImageUrl = "" },
            new Sport { Id = Guid.NewGuid(), Name = "Beach Tennis", Description = "Beach Tennis em quadra de areia", ImageUrl = "" },
            new Sport { Id = Guid.NewGuid(), Name = "Padel", Description = "Padel em quadra fechada", ImageUrl = "" },
            new Sport { Id = Guid.NewGuid(), Name = "Tênis", Description = "Tênis em quadra de saibro ou dura", ImageUrl = "" },
            new Sport { Id = Guid.NewGuid(), Name = "Vôlei de Praia", Description = "Vôlei em quadra de areia", ImageUrl = "" },
            new Sport { Id = Guid.NewGuid(), Name = "Basquete", Description = "Basquete em quadra coberta", ImageUrl = "" },
        };

        db.Sports.AddRange(defaultSports);
        await db.SaveChangesAsync(ct);
    }

    private async Task SeedOwnerUserAsync(ApplicationDbContext db, Tenant tenant, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(tenant.OwnerEmail))
        {
            _logger.LogWarning("OwnerEmail não fornecido. O usuário dono não será criado automaticamente.");
            return;
        }

        var exists = await db.Users.AnyAsync(u => u.Email == tenant.OwnerEmail, ct);
        if (exists)
            return;

        var nameParts = tenant.OwnerFirstName ?? "Owner";
        var lastNameParts = tenant.OwnerLastName ?? "Tenant";

        string defaultPassword = "Owner@123";
        var passwordHash = _passwordService.HashPassword(defaultPassword, out var salt);

        var ownerUser = new User
        {
            Id = Guid.NewGuid(),
            Email = tenant.OwnerEmail,
            FirstName = nameParts,
            LastName = lastNameParts,
            PasswordHash = passwordHash,
            Salt = salt,
            Role = Domain.Enums.UserRole.Owner,
            IsActive = true
        };

        // Preenche campos de auditoria que são NOT NULL
        ownerUser.SetCreated(Guid.Empty);

        db.Users.Add(ownerUser);
        await db.SaveChangesAsync(ct);
        _logger.LogInformation("Usuário Owner {Email} criado com sucesso para o tenant {Slug}", tenant.OwnerEmail, tenant.Slug);
    }

    public async Task ProvisionOwnerUserAsync(Tenant tenant, CancellationToken ct = default)
    {
        var connectionString = _globalCtx.Database.GetConnectionString()!;
        var factory = new ApplicationDbContextFactory(connectionString);

        await using var tenantDb = factory.CreateForTenant(tenant);

        await SeedOwnerUserAsync(tenantDb, tenant, ct);
    }
}
