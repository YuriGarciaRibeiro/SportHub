using FluentValidation;

namespace Application.UseCases.Auth.UpdateCurrentUser;

public class UpdateCurrentUserValidator : AbstractValidator<UpdateCurrentUserCommand>
{
    public UpdateCurrentUserValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("O nome é obrigatório.")
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("O sobrenome é obrigatório.")
            .MaximumLength(100);
    }
}
