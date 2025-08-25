using Application.Extensions.Validation;
using FluentValidation;

namespace Application.UseCases.Auth.Register;

public class RegisterUserValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MinimumLength(2);
        RuleFor(x => x.LastName).NotEmpty().MinimumLength(2);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
         RuleFor(x => x.Password)
            .StrongPassword(
                forbiddenSubstrings: x =>
                [
                    x.Email.Split('@')[0],
                    x.FirstName,
                    x.LastName,
                ],
                policy: new PasswordValidatorExtensions.PasswordPolicyOptions
                {
                    MinimumLength = 12,
                    RequiredCategories = 3,
                    MaxRepeatedRun = 3,
                    MinSequentialLengthToReject = 4,
                    CheckCommonPasswords = true
                });
    }
}
