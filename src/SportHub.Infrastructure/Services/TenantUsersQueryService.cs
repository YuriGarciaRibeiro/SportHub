using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class TenantUsersQueryService : ITenantUsersQueryService
{
    private readonly TenantDbContext _globalCtx;

    public TenantUsersQueryService(TenantDbContext globalCtx)
    {
        _globalCtx = globalCtx;
    }

    public async Task<List<UserDto>> GetAdminUsersAsync(Tenant tenant, CancellationToken ct = default)
    {
        var connectionString = _globalCtx.Database.GetConnectionString()!;
        var factory = new ApplicationDbContextFactory(connectionString);

        await using var tenantDb = factory.CreateForTenant(tenant);

        var users = await tenantDb.Users
            .Select(u => new UserDto(
                u.Id,
                u.FirstName,
                u.LastName,
                u.Email,
                u.Role.ToString()
            ))
            .ToListAsync(ct);

        return users;
    }
}
