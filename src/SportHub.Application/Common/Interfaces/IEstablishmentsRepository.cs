using Application.UseCases.Establishments.GetEstablishments;
using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IEstablishmentsRepository : IBaseRepository<Establishment>
{
    public Task<List<Establishment>> GetByIdsWithDetailsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken);
    public Task<Establishment?> GetByIdWithAddressAsync(Guid id, CancellationToken cancellationToken);
    Task<(List<EstablishmentResponse> Items, int TotalCount)> GetFilteredAsync(GetEstablishmentsQuery query, CancellationToken cancellationToken);
    Task<List<User>> GetUsersByEstablishmentId(Guid establishmentId, CancellationToken cancellationToken);
    Task<List<Sport>> GetSportsByEstablishmentIdAsync(Guid establishmentId, CancellationToken cancellationToken);
}
