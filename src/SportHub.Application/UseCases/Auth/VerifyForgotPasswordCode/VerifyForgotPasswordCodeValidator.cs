namespace Application.UseCases.Auth.VerifyForgotPasswordCode;

public class VerifyForgotPasswordCodeValidator : AbstractValidator<VerifyForgotPasswordCodeCommand>
{
    public VerifyForgotPasswordCodeValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.");
    }
}