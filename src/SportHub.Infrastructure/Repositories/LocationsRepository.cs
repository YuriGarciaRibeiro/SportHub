using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class LocationsRepository : ILocationsRepository
{
    private readonly DbSet<Location> _dbSet;

    public LocationsRepository(ApplicationDbContext dbContext)
    {
        _dbSet = dbContext.Set<Location>();
    }

    public async Task<List<Location>> GetAllAsync()
    {
        return await _dbSet.AsNoTracking()
            .OrderBy(l => l.Name)
            .ToListAsync();
    }

    public async Task<Location?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<Location?> GetDefaultAsync()
    {
        return await _dbSet.FirstOrDefaultAsync(l => l.IsDefault);
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
