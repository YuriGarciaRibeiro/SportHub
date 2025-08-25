using Application.Common.Interfaces.Base;
using Application.Common.QueryFilters;
using Application.UseCases.Establishments.GetEstablishments;
using Domain.Entities;

namespace Application.Common.Interfaces.Establishments;

public interface IEstablishmentsRepository : IBaseRepository<Establishment>
{
    public Task<List<Establishment>> GetByIdsWithDetailsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken);
    public Task<Establishment?> GetByIdWithAddressAsync(Guid id, CancellationToken cancellationToken);
    Task<(List<EstablishmentResponse> Items, int TotalCount)> GetFilteredAsync(GetEstablishmentsQuery query, CancellationToken cancellationToken);
    Task<List<User>> GetUsersByEstablishmentId(Guid establishmentId, CancellationToken cancellationToken);
    Task<List<Sport>> GetSportsByEstablishmentIdAsync(Guid establishmentId, CancellationToken cancellationToken);
    Task<List<Reservation>> GetReservationsByCourtsIdAsync(IEnumerable<Guid> courtIds, EstablishmentReservationsQueryFilter filter, CancellationToken cancellationToken);
    Task<EstablishmentCompleteDto?> GetByIdCompleteAsync(Guid id, CancellationToken ct = default);
}
