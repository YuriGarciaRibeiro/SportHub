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

    public SportsRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
        _dbSet = dbContext.Set<Sport>();
    }

    public async Task<Sport?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<List<Sport>> GetAllAsync()
    {
        return await _dbSet.AsNoTracking().ToListAsync();
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
        return await _dbSet.Where(e => ids.Contains(e.Id)).ToListAsync();
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _dbSet.AnyAsync(e => e.Id == id);
    }

    public IQueryable<Sport> Query()
    {
        return _dbSet.AsQueryable();
    }

    public Task AddManyAsync(IEnumerable<Sport> entities)
    {
        _dbSet.AddRange(entities);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await _dbContext.Sports
            .AnyAsync(s => EF.Functions.ILike(s.Name, name));
    }

    public async Task<Sport?> GetByNameAsync(string name)
    {
        return await _dbContext.Sports
            .FirstOrDefaultAsync(s => EF.Functions.ILike(s.Name, name));
    }

    public async Task<IEnumerable<Sport>> GetSportsByIdsAsync(IEnumerable<Guid> ids)
    {
        return await _dbContext.Sports
            .Where(s => ids.Contains(s.Id))
            .ToListAsync();
    }

    public async Task<PagedResult<Sport>> GetPagedAsync(
        int page,
        int pageSize,
        string? name = null,
        string? searchTerm = null)
    {
        var query = _dbSet.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(s => s.Name.Contains(name));

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
