using Domain.Entities;

namespace Application.Common.Interfaces;

public interface ISportsRepository : IBaseRepository<Sport>
{
    Task<Sport?> GetByNameAsync(string name, CancellationToken cancellationToken);
    Task<IEnumerable<Sport>> GetSportsByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken);
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken);
    Task<IEnumerable<Sport>> GetByEstablishmentIdAsync(Guid establishmentId, CancellationToken cancellationToken);
}
