using Domain.Enums;
using FluentValidation;

namespace Application.UseCases.Members.UpdateMemberRole;

public class UpdateMemberRoleValidator : AbstractValidator<UpdateMemberRoleCommand>
{
    public UpdateMemberRoleValidator()
    {
        RuleFor(x => x.MemberId)
            .NotEmpty()
            .WithMessage("Member ID is required");

        RuleFor(x => x.NewRole)
            .IsInEnum()
            .WithMessage("Invalid role")
            .Must(role => role != UserRole.SuperAdmin)
            .WithMessage("Cannot assign SuperAdmin role")
            .Must(role => role >= UserRole.Customer && role <= UserRole.Owner)
            .WithMessage("Role must be Customer, Staff, Manager, or Owner");
    }
}
