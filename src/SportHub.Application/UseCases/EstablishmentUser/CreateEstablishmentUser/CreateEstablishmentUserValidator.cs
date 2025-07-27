using FluentValidation;

namespace Application.UseCases.EstablishmentUser.CreateEstablishmentUser;

public class CreateEstablishmentUserValidator : AbstractValidator<CreateEstablishmentUserCommand>
{
    public CreateEstablishmentUserValidator()
    {
        RuleFor(command => command.Request)
            .NotNull()
            .WithMessage("Request cannot be null.");

        RuleFor(command => command.Request.Users)
            .NotEmpty()
            .WithMessage("At least one user must be provided.")
            .ForEach(user =>
            {
                user.NotNull().WithMessage("User cannot be null.");
                user.SetValidator(new EstablishmentUserRequestValidator());
            });

        RuleFor(command => command.EstablishmentId)
            .NotEmpty()
            .WithMessage("Establishment ID cannot be empty.");
    }
}


public class EstablishmentUserRequestValidator : AbstractValidator<EstablishmentUserRequest>
{
    public EstablishmentUserRequestValidator()
    {
        RuleFor(u => u.UserId).NotEmpty().WithMessage("User ID cannot be empty.");
        RuleFor(u => u.Role).IsInEnum().WithMessage("Invalid establishment role.");
    }
}
