using FluentValidation;

namespace Application.Extensions.Validation;

public static class PasswordValidatorExtensions
{
    public static IRuleBuilderOptions<T, string> Password<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .MinimumLength(8)
            .Matches(@"^(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+$")
            .WithMessage("Password must be at least 8 characters long, contain at least one uppercase letter, one digit, and one special character.");
    }
}
