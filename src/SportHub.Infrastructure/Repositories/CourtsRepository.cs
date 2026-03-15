using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CourtsRepository : ICourtsRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly DbSet<Court> _dbSet;

    public CourtsRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
        _dbSet = dbContext.Set<Court>();
    }

    public async Task<Court?> GetByIdAsync(Guid id)
    {
        return await _dbContext.Courts
            .Include(c => c.Sports)
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<List<Court>> GetAllAsync()
    {
        return await _dbContext.Courts
            .Include(c => c.Sports)
            .AsSplitQuery()
            .AsNoTracking()
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

    public async Task<List<Court>> GetByIdsAsync(IEnumerable<Guid> ids) =>
        await _dbSet.Where(e => ids.Contains(e.Id)).ToListAsync();

    public async Task<bool> ExistsAsync(Guid id) =>
        await _dbSet.AnyAsync(e => e.Id == id);

    public IQueryable<Court> Query() =>
        _dbSet.AsQueryable();

    public Task AddManyAsync(IEnumerable<Court> entities)
    {
        _dbSet.AddRange(entities);
        return Task.CompletedTask;
    }
}
