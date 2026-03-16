using Application.Common.Interfaces;
using Application.Common.Models;
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

        return await tenantDb.Users
            .Select(u => new UserDto(u.Id, u.FirstName, u.LastName, u.Email, u.Role.ToString()))
            .ToListAsync(ct);
    }

    public async Task<PagedResult<UserDto>> GetPagedUsersAsync(Tenant tenant, int page, int pageSize, string? searchTerm = null, string? role = null, CancellationToken ct = default)
    {
        var connectionString = _globalCtx.Database.GetConnectionString()!;
        var factory = new ApplicationDbContextFactory(connectionString);

        await using var tenantDb = factory.CreateForTenant(tenant);

        var query = tenantDb.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(u =>
                u.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                u.FirstName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                u.LastName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(role))
            query = query.Where(u => u.Role.ToString() == role);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserDto(u.Id, u.FirstName, u.LastName, u.Email, u.Role.ToString()))
            .ToListAsync(ct);

        return new PagedResult<UserDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
