using FluentValidation;

namespace Application.UseCases.Reservations.CreateReservation;

public class CreateReservationValidator : AbstractValidator<CreateReservationCommand>
{
    public CreateReservationValidator()
    {
        RuleFor(x => x.CourtId).NotEmpty();
        RuleFor(x => x.Reservation).NotNull();
        RuleFor(x => x.Reservation.StartTime).NotEmpty();
        RuleFor(x => x.Reservation.EndTime).NotEmpty();
    }
}
