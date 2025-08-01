using Application.CQRS;

namespace Application.UseCases.Reservations.CreateReservation;

public class CreateReservationCommand : ICommand<CreateReservationResponse>
{
    public Guid CourtId { get; set; }
    public ReservationRequest Reservation { get; set; } = null!;
}

public class ReservationRequest
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}
