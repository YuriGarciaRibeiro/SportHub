using Application.Common.Interfaces;
using Application.Common.QueryFilters;
using Domain.Entities;

namespace Application.Common.Interfaces;

public interface ICourtService : IBaseService<Court>
{
    Task<List<Court>> GetCourtsByEstablishmentIdAsync(Guid establishmentId, CancellationToken ct = default);
    Task<List<Guid>> GetCourtIdsByEstablishmentIdAsync(Guid establishmentId, CancellationToken ct = default);
    Task<List<Court>> GetAvailableCourtsAsync(Guid establishmentId, DateTime startTime, DateTime endTime, CancellationToken ct = default);
    Task<Court?> GetCompleteByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Court>> GetByFilterAsync(CourtQueryFilter filter, CancellationToken ct = default);
}
