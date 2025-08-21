namespace Application.UseCases.Establishments.GetEstablishmentSports;

public class GetEstablishmentSportsValidator : AbstractValidator<GetEstablishmentSportsQuery>
{
    public GetEstablishmentSportsValidator()
    {
        RuleFor(x => x.EstablishmentId).NotEmpty().WithMessage("Establishment ID must not be empty.");
    }
}
