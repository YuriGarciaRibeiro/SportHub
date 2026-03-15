using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

/// <summary>
/// Repositório de tenants. Opera SEMPRE no schema "public" via TenantDbContext.
/// Implementa métodos diretamente porque Tenant não implementa IEntity.
/// </summary>
public class TenantRepository : ITenantRepository
{
    private readonly TenantDbContext _context;

    public TenantRepository(TenantDbContext context)
    {
        _context = context;
    }

    public async Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<Tenant?> GetBySlugAsync(string slug, CancellationToken ct = default) =>
        await _context.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Slug == slug.ToLowerInvariant(), ct);

    public async Task<Tenant?> GetByCustomDomainAsync(string domain, CancellationToken ct = default) =>
        await _context.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.CustomDomain == domain.ToLowerInvariant(), ct);

    public async Task AddAsync(Tenant tenant, CancellationToken ct = default)
    {
        await _context.Tenants.AddAsync(tenant, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Tenant tenant, CancellationToken ct = default)
    {
        _context.Tenants.Update(tenant);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default) =>
        await _context.Tenants.AnyAsync(t => t.Slug == slug.ToLowerInvariant(), ct);

    public async Task<List<Tenant>> GetAllAsync(CancellationToken ct = default) =>
        await _context.Tenants
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .ToListAsync(ct);

    public async Task<PagedResult<Tenant>> GetPagedAsync(
        int page,
        int pageSize,
        string? name = null,
        string? slug = null,
        TenantStatus? status = null,
        string? searchTerm = null,
        CancellationToken ct = default)
    {
        var query = _context.Tenants.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(t => t.Name.Contains(name));
        }

        if (!string.IsNullOrWhiteSpace(slug))
        {
            query = query.Where(t => t.Slug.Contains(slug.ToLowerInvariant()));
        }

        if (status.HasValue)
        {
            query = query.Where(t => t.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var search = searchTerm.ToLower();
            query = query.Where(t => 
                t.Name.ToLower().Contains(search) ||
                t.Slug.ToLower().Contains(search) ||
                (t.OwnerEmail != null && t.OwnerEmail.ToLower().Contains(search)));
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(t => t.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Tenant>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
