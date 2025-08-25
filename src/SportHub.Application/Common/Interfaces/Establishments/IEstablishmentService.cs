using Application.Common.Interfaces.Base;
using Application.Common.QueryFilters;
using Application.UseCases.Establishments.GetEstablishments;
using Application.Common.Interfaces.Reservations;
using Application.Common.Interfaces.Sports;
using Domain.Entities;

namespace Application.Common.Interfaces.Establishments;

public interface IEstablishmentService : IBaseService<Establishment>
{
    Task<EstablishmentCompleteDto?> GetByIdCompleteAsync(Guid id, CancellationToken ct = default);
    Task<Result<List<Establishment>>> GetEstablishmentsByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken);
    Task<(List<EstablishmentResponse> Items, int TotalCount)> GetFilteredAsync(GetEstablishmentsQuery query, CancellationToken cancellationToken);
    Task<EstablishmentWithAddressDto?> GetByIdWithAddressAsync(Guid id, CancellationToken ct = default);
    Task<List<EstablishmentUserSummaryDto>> GetUsersByEstablishmentIdAsync(Guid establishmentId, CancellationToken ct = default);
    Task<List<SportSummaryDto>> GetSportsByEstablishmentIdAsync(Guid establishmentId, CancellationToken ct = default);
    Task<List<ReservationWithDetailsDto>> GetReservationsByCourtsIdAsync(IEnumerable<Guid> courtIds, EstablishmentReservationsQueryFilter filter, CancellationToken ct = default);
}
