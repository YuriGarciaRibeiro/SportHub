using Application.UseCases.Establishments.GetEstablishments;
using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IEstablishmentsRepository : IBaseRepository<Establishment>
{
    public Task<List<Establishment>> GetByIdsWithDetailsAsync(IEnumerable<Guid> ids);
    public Task<Establishment?> GetByIdWithAddressAsync(Guid id);
    Task<(List<EstablishmentResponse> Items, int TotalCount)> GetFilteredAsync(GetEstablishmentsQuery query, CancellationToken cancellationToken);
    Task<List<User>> GetUsersByEstablishmentId(Guid establishmentId);
}
