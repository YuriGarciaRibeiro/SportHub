using Application.Common.Interfaces.Base;
using Application.Common.QueryFilters;
using Domain.Entities;

namespace Application.Common.Interfaces.Courts;

public interface ICourtsRepository : IBaseRepository<Court>
{
    // DTOs methods
    Task<IEnumerable<CourtWithSportsDto>> GetByEstablishmentIdAsync(Guid establishmentId, CancellationToken cancellationToken);
    Task<IEnumerable<CourtFilterResultDto>> GetByFilterAsync(CourtQueryFilter filter, CancellationToken cancellationToken);
    Task<CourtCompleteDto?> GetCompleteByIdAsync(Guid id, CancellationToken cancellationToken);
    
    // Simple methods (no DTOs needed)
    Task<IEnumerable<Guid>> GetCourtIdsByEstablishmentIdAsync(Guid establishmentId, CancellationToken cancellationToken);
}

