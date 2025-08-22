namespace Application.UseCases.Establishments.GetEstablishmentReservations;

public class GetEstablishmentReservationsValidator : AbstractValidator<GetEstablishmentReservationsQuery>
{
    public GetEstablishmentReservationsValidator()
    {
        RuleFor(x => x.EstablishmentId)
            .NotEmpty().WithMessage("Establishment ID must not be empty.")
            .Must(BeAValidGuid).WithMessage("Establishment ID must be a valid GUID.");

        When(x => x.Filter != null, () =>
        {
            RuleFor(x => x.Filter.StartTime)
                .LessThanOrEqualTo(x => x.Filter.EndTime)
                .WithMessage("Start time must be less than or equal to end time.");

            RuleFor(x => x.Filter.EndTime)
                .GreaterThan(x => x.Filter.StartTime)
                .WithMessage("End time must be greater than start time.");
        });
    }

    private bool BeAValidGuid(Guid id) => id != Guid.Empty;
}