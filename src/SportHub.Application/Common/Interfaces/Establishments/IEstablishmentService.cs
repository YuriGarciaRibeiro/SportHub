using Application.Common.Interfaces.Base;
using Application.Common.QueryFilters;
using Application.UseCases.Establishments.GetEstablishments;
using Domain.Entities;

namespace Application.Common.Interfaces.Establishments;

public interface IEstablishmentService : IBaseService<Establishment>
{
    Task<EstablishmentCompleteDto?> GetByIdCompleteAsync(Guid id, CancellationToken ct = default);
    Task<Result<List<Establishment>>> GetEstablishmentsByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken);
    Task<(List<EstablishmentResponse> Items, int TotalCount)> GetFilteredAsync(GetEstablishmentsQuery query, CancellationToken cancellationToken);
    Task<Establishment?> GetByIdWithAddressAsync(Guid id, CancellationToken ct = default);
    Task<List<User>> GetUsersByEstablishmentIdAsync(Guid establishmentId, CancellationToken ct = default);
    Task<List<Sport>> GetSportsByEstablishmentIdAsync(Guid establishmentId, CancellationToken ct = default);
    Task<List<Reservation>> GetReservationsByCourtsIdAsync(IEnumerable<Guid> courtIds, EstablishmentReservationsQueryFilter filter, CancellationToken ct = default);
}
