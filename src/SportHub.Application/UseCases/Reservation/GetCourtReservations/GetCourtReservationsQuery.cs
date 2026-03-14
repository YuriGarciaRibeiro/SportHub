using Application.CQRS;
using Application.UseCases.Reservations.GetMyReservations;

namespace Application.UseCases.Reservations.GetCourtReservations;

public record GetCourtReservationsQuery(Guid CourtId, DateTime? Date) : IQuery<List<ReservationResponse>>;
