using FluentValidation;

namespace Application.UseCases.EstablishmentUser.CreateEstablishmentUser;

public class CreateEstablishmentUserValidator : AbstractValidator<CreateEstablishmentUserCommand>
{
    public CreateEstablishmentUserValidator()
    {
        RuleFor(command => command.Users)
            .NotEmpty().WithMessage("At least one user must be provided.")
            .Must(users => users.All(user => user.UserId != Guid.Empty))
            .WithMessage("User ID cannot be empty.");

        RuleFor(command => command.EstablishmentId)
            .NotEmpty().WithMessage("Establishment ID cannot be empty.");
    }
}
