using Application.Common.QueryFilters;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using CourtDtos = Application.Common.Interfaces.Courts;

namespace Infrastructure.Repositories;

public class CourtsRepository : BaseRepository<Court>, ICourtsRepository
{
    private readonly ApplicationDbContext _dbContext;

    public CourtsRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<CourtDtos.CourtWithSportsDto>> GetByEstablishmentIdAsync(Guid establishmentId, CancellationToken cancellationToken)
    {
        return await _dbContext.Courts
            .Where(c => c.EstablishmentId == establishmentId)
            .Select(c => new CourtDtos.CourtWithSportsDto(
                c.Id,
                c.Name,
                c.MinBookingSlots,
                c.MaxBookingSlots,
                c.SlotDurationMinutes,
                c.OpeningTime,
                c.ClosingTime,
                c.TimeZone,
                c.Sports.Select(s => new CourtDtos.SportDto(
                    s.Id,
                    s.Name,
                    s.Description
                ))
            ))
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<CourtDtos.CourtFilterResultDto>> GetByFilterAsync(CourtQueryFilter filter, CancellationToken cancellationToken)
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
            .Select(c => new CourtDtos.CourtFilterResultDto(
                c.Id,
                c.Name,
                c.MinBookingSlots,
                c.MaxBookingSlots,
                c.SlotDurationMinutes,
                c.OpeningTime,
                c.ClosingTime,
                c.TimeZone,
                new CourtDtos.EstablishmentSummaryDto(
                    c.Establishment.Id,
                    c.Establishment.Name,
                    c.Establishment.Description,
                    c.Establishment.ImageUrl
                ),
                c.Sports.Select(s => new CourtDtos.SportDto(
                    s.Id,
                    s.Name,
                    s.Description
                ))
            ))
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<CourtDtos.CourtCompleteDto?> GetCompleteByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _dbContext.Courts
            .Where(c => c.Id == id)
            .Select(c => new CourtCompleteDto(
                c.Id,
                c.Name,
                c.MinBookingSlots,
                c.MaxBookingSlots,
                c.SlotDurationMinutes,
                c.OpeningTime,
                c.ClosingTime,
                c.TimeZone,
                new CourtDtos.EstablishmentSummaryDto(
                    c.Establishment.Id,
                    c.Establishment.Name,
                    c.Establishment.Description,
                    c.Establishment.ImageUrl
                ),
                c.Sports.Select(s => new CourtDtos.SportDto(
                    s.Id,
                    s.Name,
                    s.Description
                ))
            ))
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);
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
