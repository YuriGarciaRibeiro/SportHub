using Application.Common.QueryFilters;
using Domain.Entities;

namespace Application.Common.Interfaces;

public interface ICourtsRepository : IBaseRepository<Court>
{
    Task<IEnumerable<Court>> GetByEstablishmentIdAsync(Guid establishmentId, CancellationToken cancellationToken);
    Task<IEnumerable<Guid>> GetCourtIdsByEstablishmentIdAsync(Guid establishmentId, CancellationToken cancellationToken);
    Task<IEnumerable<Court>> GetByFilterAsync(CourtQueryFilter filter, CancellationToken cancellationToken);
    Task<Court?> GetCompleteByIdAsync(Guid id, CancellationToken cancellationToken);
}

