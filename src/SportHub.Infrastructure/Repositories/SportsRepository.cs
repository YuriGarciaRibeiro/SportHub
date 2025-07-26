using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class SportsRepository : BaseRepository<Sport>, ISportsRepository
{
    private readonly ApplicationDbContext _dbContext;
    public SportsRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
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
}
