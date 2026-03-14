using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

/// <summary>
/// Repositório de tenants. Opera SEMPRE no schema "public" via TenantDbContext.
/// Não herda BaseRepository porque Tenant não implementa IEntity.
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
}
