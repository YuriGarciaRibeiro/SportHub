using FluentValidation;

namespace Application.UseCases.Establishments.GetNearbyEstablishments;

public class GetNearbyEstablishmentsValidator : AbstractValidator<GetNearbyEstablishmentsQuery>
{
    public GetNearbyEstablishmentsValidator()
    {
        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90)
            .WithMessage("Latitude must be between -90 and 90 degrees.");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180)
            .WithMessage("Longitude must be between -180 and 180 degrees.");

        RuleFor(x => x.RadiusKm)
            .GreaterThan(0)
            .LessThanOrEqualTo(100)
            .WithMessage("Radius must be between 0.1 and 100 kilometers.");
    }
}
