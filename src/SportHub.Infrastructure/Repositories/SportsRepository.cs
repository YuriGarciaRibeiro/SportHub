using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class SportsRepository : ISportsRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly DbSet<Sport> _dbSet;
    private readonly ITenantContext _tenantContext;

    public SportsRepository(ApplicationDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _dbSet = dbContext.Set<Sport>();
        _tenantContext = tenantContext;
    }

    public async Task<Sport?> GetByIdAsync(Guid id)
    {
        var tenantId = _tenantContext.TenantId;
        return await _dbSet.FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId);
    }

    public async Task<List<Sport>> GetAllAsync()
    {
        var tenantId = _tenantContext.TenantId;
        return await _dbSet.AsNoTracking().Where(s => s.TenantId == tenantId).ToListAsync();
    }

    public Task AddAsync(Sport entity)
    {
        _dbSet.Add(entity);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Sport entity)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(Sport entity)
    {
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<List<Sport>> GetByIdsAsync(IEnumerable<Guid> ids)
    {
        var tenantId = _tenantContext.TenantId;
        return await _dbSet.Where(e => ids.Contains(e.Id) && e.TenantId == tenantId).ToListAsync();
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        var tenantId = _tenantContext.TenantId;
        return await _dbSet.AnyAsync(e => e.Id == id && e.TenantId == tenantId);
    }

    public IQueryable<Sport> Query()
    {
        var tenantId = _tenantContext.TenantId;
        return _dbSet.Where(s => s.TenantId == tenantId);
    }

    public Task AddManyAsync(IEnumerable<Sport> entities)
    {
        _dbSet.AddRange(entities);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsByNameAsync(string name)
    {
        var tenantId = _tenantContext.TenantId;
        return await _dbContext.Sports
            .AnyAsync(s => s.TenantId == tenantId && EF.Functions.ILike(s.Name, name));
    }

    public async Task<Sport?> GetByNameAsync(string name)
    {
        var tenantId = _tenantContext.TenantId;
        return await _dbContext.Sports
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && EF.Functions.ILike(s.Name, name));
    }

    public async Task<IEnumerable<Sport>> GetSportsByIdsAsync(IEnumerable<Guid> ids)
    {
        var tenantId = _tenantContext.TenantId;
        return await _dbContext.Sports
            .Where(s => s.TenantId == tenantId && ids.Contains(s.Id))
            .ToListAsync();
    }

    public async Task<PagedResult<Sport>> GetPagedAsync(
        int page,
        int pageSize,
        string? name = null,
        string? searchTerm = null)
    {
        var tenantId = _tenantContext.TenantId;
        var query = _dbSet.AsNoTracking().Where(s => s.TenantId == tenantId).AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(s => s.Name.Contains(name));
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var search = searchTerm.ToLower();
            query = query.Where(s => 
                s.Name.ToLower().Contains(search) ||
                s.Description.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(s => s.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Sport>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
