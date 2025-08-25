using Application.Extensions.Validation;
using FluentValidation;

namespace Application.UseCases.Auth.Login;

public class LoginValidator : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
         RuleFor(x => x.Password)
            .StrongPassword(
                policy: new PasswordValidatorExtensions.PasswordPolicyOptions
                {
                    MinimumLength = 12,
                    RequiredCategories = 3,
                    MaxRepeatedRun = 3,
                    MinSequentialLengthToReject = 4,
                    CheckCommonPasswords = true
                });;
    }
}
