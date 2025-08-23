using Application.Common.Interfaces;
using Domain.Entities;

namespace Application.Common.Interfaces;

public interface ISportService : IBaseService<Sport>
{
    Task<List<Sport>> GetSportsByEstablishmentIdAsync(Guid establishmentId, CancellationToken ct = default);
    Task<Sport?> GetSportByNameAsync(string name, CancellationToken ct = default);
    Task<List<Sport>> GetSportsByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
}
