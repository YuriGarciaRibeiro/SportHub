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

    public async Task<PagedResult<Court>> GetPagedAsync(
        int page,
        int pageSize,
        string? name = null,
        Guid? sportId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        string? searchTerm = null)
    {
        var query = _dbContext.Courts
            .Include(c => c.Sports)
            .AsSplitQuery()
            .AsNoTracking()
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
