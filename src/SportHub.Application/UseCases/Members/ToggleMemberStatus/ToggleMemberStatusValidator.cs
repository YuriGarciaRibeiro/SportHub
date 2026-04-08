using FluentValidation;

namespace Application.UseCases.Members.ToggleMemberStatus;

public class ToggleMemberStatusValidator : AbstractValidator<ToggleMemberStatusCommand>
{
    public ToggleMemberStatusValidator()
    {
        RuleFor(x => x.MemberId)
            .NotEmpty()
            .WithMessage("Member ID is required");
    }
}
