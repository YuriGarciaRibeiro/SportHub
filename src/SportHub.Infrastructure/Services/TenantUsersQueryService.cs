using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class TenantUsersQueryService : ITenantUsersQueryService
{
    private readonly ApplicationDbContext _db;

    public TenantUsersQueryService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<UserDto>> GetAdminUsersAsync(Tenant tenant, CancellationToken ct = default)
    {
        return await _db.Users
            .IgnoreQueryFilters()
            .Where(u => u.TenantId == tenant.Id && !u.IsDeleted)
            .Select(u => new UserDto(u.Id, u.FirstName, u.LastName, u.Email, u.Role.ToString()))
            .ToListAsync(ct);
    }

    public async Task<PagedResult<UserDto>> GetPagedUsersAsync(Tenant tenant, int page, int pageSize, string? searchTerm = null, string? role = null, CancellationToken ct = default)
    {
        var query = _db.Users
            .IgnoreQueryFilters()
            .Where(u => u.TenantId == tenant.Id && !u.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(u =>
                u.Email.Contains(searchTerm) ||
                u.FirstName.Contains(searchTerm) ||
                u.LastName.Contains(searchTerm));

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
