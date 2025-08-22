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

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken)
    {
        return await _dbContext.Sports
            .AnyAsync(s => EF.Functions.ILike(s.Name, name), cancellationToken);
    }

    public async Task<Sport?> GetByNameAsync(string name, CancellationToken cancellationToken)
    {
        return await _dbContext.Sports
            .FirstOrDefaultAsync(s => EF.Functions.ILike(s.Name, name), cancellationToken);
    }

    public async Task<IEnumerable<Sport>> GetSportsByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
    {
        return await _dbContext.Sports
            .Where(s => ids.Contains(s.Id))
            .ToListAsync(cancellationToken);
    }
}
