using Application.CQRS;

namespace Application.UseCases.Reservations.CancelReservation;

public record CancelReservationCommand(Guid ReservationId) : ICommand;
