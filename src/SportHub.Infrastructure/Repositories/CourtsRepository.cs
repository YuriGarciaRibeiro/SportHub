using Application.Common.QueryFilters;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CourtsRepository : BaseRepository<Court>, ICourtsRepository
{
    private readonly ApplicationDbContext _dbContext;

    public CourtsRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<Court>> GetByEstablishmentIdAsync(Guid establishmentId, CancellationToken cancellationToken)
    {
        return await _dbContext.Courts
            .Where(c => c.EstablishmentId == establishmentId)
            .Include(c => c.Sports)
            .AsSplitQuery()
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Court>> GetByFilterAsync(CourtQueryFilter filter, CancellationToken cancellationToken)
    {
        var query = _dbContext.Courts.AsQueryable();

        if (!string.IsNullOrEmpty(filter.Name))
        {
            query = query.Where(c => c.Name.Contains(filter.Name));
        }

        if (filter.OpeningTime.HasValue)
        {
            query = query.Where(c => c.OpeningTime == filter.OpeningTime.Value);
        }

        if (filter.ClosingTime.HasValue)
        {
            query = query.Where(c => c.ClosingTime == filter.ClosingTime.Value);
        }

        if (filter.SportIds != null && filter.SportIds.Any())
        {
            query = query.Where(c => c.Sports.Any(s => filter.SportIds.Contains(s.Id)));
        }

        return await query
                    .Include(c => c.Sports)
                    .Include(c => c.Establishment)
                    .AsSplitQuery()
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);
    }

    public Task<Court?> GetCompleteByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _dbContext.Courts
            .Include(c => c.Sports)
            .Include(c => c.Establishment)
            .AsSplitQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Guid>> GetCourtIdsByEstablishmentIdAsync(Guid establishmentId, CancellationToken cancellationToken)
    {
        return await _dbContext.Courts
            .Where(c => c.EstablishmentId == establishmentId)
            .AsNoTracking()
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);
    }
}
