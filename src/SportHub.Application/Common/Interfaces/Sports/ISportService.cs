using Domain.Entities;

namespace Application.Common.Interfaces.Sports;

public interface ISportService : IBaseService<Sport>
{
    Task<IEnumerable<Sport>> GetSportsByEstablishmentIdAsync(Guid establishmentId, CancellationToken ct = default);
    Task<Sport?> GetSportByNameAsync(string name, CancellationToken ct = default);
}
