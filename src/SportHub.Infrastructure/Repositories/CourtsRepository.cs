using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CourtsRepository : ICourtsRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly DbSet<Court> _dbSet;
    private readonly ITenantContext _tenantContext;

    public CourtsRepository(ApplicationDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _dbSet = dbContext.Set<Court>();
        _tenantContext = tenantContext;
    }

    public async Task<Court?> GetByIdAsync(Guid id)
    {
        var tenantId = _tenantContext.TenantId;
        return await _dbContext.Courts
            .Include(c => c.Sports)
            .Include(c => c.Location)
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId);
    }

    public async Task<List<Court>> GetAllAsync()
    {
        var tenantId = _tenantContext.TenantId;
        return await _dbContext.Courts
            .Include(c => c.Sports)
            .Include(c => c.Location)
            .AsSplitQuery()
            .AsNoTracking()
            .Where(c => c.TenantId == tenantId)
            .ToListAsync();
    }

    public Task AddAsync(Court entity)
    {
        _dbSet.Add(entity);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Court entity)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(Court entity)
    {
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<List<Court>> GetByIdsAsync(IEnumerable<Guid> ids)
    {
        var tenantId = _tenantContext.TenantId;
        return await _dbSet.Where(e => ids.Contains(e.Id) && e.TenantId == tenantId).ToListAsync();
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        var tenantId = _tenantContext.TenantId;
        return await _dbSet.AnyAsync(e => e.Id == id && e.TenantId == tenantId);
    }

    public IQueryable<Court> Query()
    {
        var tenantId = _tenantContext.TenantId;
        return _dbSet.Where(c => c.TenantId == tenantId);
    }

    public Task AddManyAsync(IEnumerable<Court> entities)
    {
        _dbSet.AddRange(entities);
        return Task.CompletedTask;
    }

    public async Task<PagedResult<Court>> GetPagedAsync(
        int page,
        int pageSize,
        string? name = null,
        Guid? sportId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        string? searchTerm = null,
        Guid? locationId = null)
    {
        var tenantId = _tenantContext.TenantId;
        var query = _dbContext.Courts
            .Include(c => c.Sports)
            .Include(c => c.Location)
            .AsSplitQuery()
            .AsNoTracking()
            .Where(c => c.TenantId == tenantId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(c => c.Name.Contains(name));
        }

        if (sportId.HasValue)
        {
            query = query.Where(c => c.Sports.Any(s => s.Id == sportId.Value));
        }

        if (minPrice.HasValue)
        {
            query = query.Where(c => c.PricePerHour >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(c => c.PricePerHour <= maxPrice.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var search = searchTerm.ToLower();
            query = query.Where(c => c.Name.ToLower().Contains(search));
        }

        if (locationId.HasValue)
        {
            query = query.Where(c => c.LocationId == locationId.Value);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Court>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
