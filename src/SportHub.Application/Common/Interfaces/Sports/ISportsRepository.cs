using Application.Common.Interfaces.Base;
using Domain.Entities;

namespace Application.Common.Interfaces.Sports;

public interface ISportsRepository : IBaseRepository<Sport>
{
    // DTOs methods
    Task<IEnumerable<SportBulkDto>> GetSportsByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken);
    Task<IEnumerable<SportSummaryDto>> GetByEstablishmentIdAsync(Guid establishmentId, CancellationToken cancellationToken);
    
    // Simple methods (no DTOs needed)
    Task<Sport?> GetByNameAsync(string name, CancellationToken cancellationToken);
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken);
}
