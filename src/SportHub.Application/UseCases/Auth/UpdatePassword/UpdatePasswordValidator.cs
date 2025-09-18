using Application.Extensions.Validation;

namespace Application.UseCases.Auth.UpdatePassword;

public class UpdatePasswordValidator : AbstractValidator<UpdatePasswordCommand>
{
    public UpdatePasswordValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.NewPassword).NotEmpty().StrongPassword();
    }
}