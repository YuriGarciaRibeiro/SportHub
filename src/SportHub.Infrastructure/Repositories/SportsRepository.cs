using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SportDtos = Application.Common.Interfaces.Sports;

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

    public async Task<IEnumerable<SportDtos.SportSummaryDto>> GetByEstablishmentIdAsync(Guid establishmentId, CancellationToken cancellationToken)
    {
        return await _dbContext.Sports
            .Where(s => s.Establishments.Any(e => e.Id == establishmentId))
            .Select(s => new SportDtos.SportSummaryDto(
                s.Id,
                s.Name,
                s.Description
            ))
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Sport?> GetByNameAsync(string name, CancellationToken cancellationToken)
    {
        return await _dbContext.Sports
            .FirstOrDefaultAsync(s => EF.Functions.ILike(s.Name, name), cancellationToken);
    }

    public async Task<IEnumerable<SportDtos.SportBulkDto>> GetSportsByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
    {
        return await _dbContext.Sports
            .Where(s => ids.Contains(s.Id))
            .Select(s => new SportDtos.SportBulkDto(
                s.Id,
                s.Name,
                s.Description
            ))
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
