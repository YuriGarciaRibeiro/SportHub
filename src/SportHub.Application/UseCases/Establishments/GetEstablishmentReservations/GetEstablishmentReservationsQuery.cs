using Application.Common.QueryFilters;

namespace Application.UseCases.Establishments.GetEstablishmentReservations;

public class GetEstablishmentReservationsQuery : IQuery<GetEstablishmentReservationsResponse>
{
    public Guid EstablishmentId { get; set; }
    public EstablishmentReservationsQueryFilter Filter { get; set; } = new EstablishmentReservationsQueryFilter();
}

