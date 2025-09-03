using Application.Common.Interfaces.Base;
using Application.Common.QueryFilters;
using Application.UseCases.Establishments.GetEstablishments;
using Domain.Entities;
using Application.Common.Interfaces.Reservations;

namespace Application.Common.Interfaces.Establishments;

public interface IEstablishmentsRepository : IBaseRepository<Establishment>
{
    // DTOs methods
    Task<EstablishmentCompleteDto?> GetByIdCompleteAsync(Guid id, CancellationToken ct = default);
    Task<List<EstablishmentWithDetailsDto>> GetByIdsWithDetailsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken);
    Task<EstablishmentWithAddressDto?> GetByIdWithAddressAsync(Guid id, CancellationToken cancellationToken);
    Task<List<EstablishmentUserSummaryDto>> GetUsersByEstablishmentId(Guid establishmentId, CancellationToken cancellationToken);
    Task<List<SportSummaryDto>> GetSportsByEstablishmentIdAsync(Guid establishmentId, CancellationToken cancellationToken);
    Task<List<ReservationWithDetailsDto>> GetReservationsByCourtsIdAsync(IEnumerable<Guid> courtIds, EstablishmentReservationsQueryFilter filter, CancellationToken cancellationToken);
    
    // Legacy methods (to be replaced gradually)
    Task<(List<EstablishmentResponse> Items, int TotalCount)> GetFilteredAsync(GetEstablishmentsQuery query, Guid? userId, CancellationToken cancellationToken);
}
