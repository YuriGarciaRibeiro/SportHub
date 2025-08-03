using FluentValidation;

namespace Application.UseCases.Establishments.UpdateEstablishmentUserRole;

public class UpdateEstablishmentUserRoleValidator : AbstractValidator<UpdateEstablishmentUserRoleCommand>
{
    public UpdateEstablishmentUserRoleValidator()
    {
        RuleFor(x => x.EstablishmentId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Request.Role).IsInEnum()
            .WithMessage("Role must be a valid establishment role.");
    }
}
