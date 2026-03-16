using Domain.Enums;
using FluentValidation;

namespace Application.UseCases.Members.UpsertMember;

public class UpsertMemberValidator : AbstractValidator<UpsertMemberCommand>
{
    public UpsertMemberValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.FirstName)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.FirstName));

        RuleFor(x => x.LastName)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.LastName));

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Invalid role.")
            .Must(r => r >= UserRole.Staff && r < UserRole.SuperAdmin)
            .WithMessage("Role must be Staff, Manager, or Owner.");
    }
}
