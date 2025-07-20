using FluentValidation;

namespace Application.UserCases.EstablishmentUser.CreateEstablishmentUser;

public class CreateEstablishmentUserValidator : AbstractValidator<CreateEstablishmentUserCommand>
{
    public CreateEstablishmentUserValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.EstablishmentId)
            .NotEmpty().WithMessage("Establishment ID is required.");

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Invalid role.");
    }
}
