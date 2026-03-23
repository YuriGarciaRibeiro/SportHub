using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class LocationsRepository : ILocationsRepository
{
    private readonly DbSet<Location> _dbSet;
    private readonly ITenantContext _tenantContext;

    public LocationsRepository(ApplicationDbContext dbContext, ITenantContext tenantContext)
    {
        _dbSet = dbContext.Set<Location>();
        _tenantContext = tenantContext;
    }

    public async Task<List<Location>> GetAllAsync()
    {
        var tenantId = _tenantContext.TenantId;
        return await _dbSet.AsNoTracking()
            .Where(l => l.TenantId == tenantId)
            .OrderBy(l => l.Name)
            .ToListAsync();
    }

    public async Task<Location?> GetByIdAsync(Guid id)
    {
        var tenantId = _tenantContext.TenantId;
        return await _dbSet.FirstOrDefaultAsync(l => l.Id == id && l.TenantId == tenantId);
    }

    public async Task<Location?> GetDefaultAsync()
    {
        var tenantId = _tenantContext.TenantId;
        return await _dbSet.FirstOrDefaultAsync(l => l.IsDefault && l.TenantId == tenantId);
    }

    public Task AddAsync(Location entity)
    {
        _dbSet.Add(entity);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Location entity)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(Location entity)
    {
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }
}
