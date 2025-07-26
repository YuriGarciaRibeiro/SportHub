using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class SportsRepository : ISportsRepository
{
    private readonly ApplicationDbContext _dbContext;

    public SportsRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Sport?> GetByIdAsync(Guid id)
    {
        return await _dbContext.Sports.FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Sport?> GetByNameAsync(string name)
    {
        return await _dbContext.Sports.FirstOrDefaultAsync(s => s.Name == name);
    }

    public async Task<IEnumerable<Sport>> GetAllAsync()
    {
        return await _dbContext.Sports.ToListAsync();
    }

    public async Task CreateAsync(Sport sport)
    {
        await _dbContext.Sports.AddAsync(sport);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(Sport sport)
    {
        _dbContext.Sports.Update(sport);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var sport = await GetByIdAsync(id);
        if (sport != null)
        {
            _dbContext.Sports.Remove(sport);
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(string name)
    {
        return await _dbContext.Sports.AnyAsync(s => s.Name == name);
    }

    public async Task<IEnumerable<Sport>> GetSportsByIdsAsync(IEnumerable<Guid> ids)
    {
        return await _dbContext.Sports
            .Where(s => ids.Contains(s.Id))
            .ToListAsync();
    }
}
