using Application.Common.QueryFilters;
using Application.Common.Interfaces.Courts;
using Domain.Entities;

namespace Application.Common.Interfaces.Courts;

public interface ICourtService : IBaseService<Court>
{
    Task<List<CourtWithSportsDto>> GetCourtsByEstablishmentIdAsync(Guid establishmentId, CancellationToken ct = default);
    Task<List<Guid>> GetCourtIdsByEstablishmentIdAsync(Guid establishmentId, CancellationToken ct = default);
    Task<List<CourtFilterResultDto>> GetAvailableCourtsAsync(Guid establishmentId, DateTime startTime, DateTime endTime, CancellationToken ct = default);
    Task<CourtCompleteDto?> GetCompleteByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<CourtFilterResultDto>> GetByFilterAsync(CourtQueryFilter filter, CancellationToken ct = default);
}
