using Application.Common.Interfaces;
using FluentValidation;

namespace Application.UseCases.Tenant.ProvisionTenant;

public class ProvisionTenantValidator : AbstractValidator<ProvisionTenantCommand>
{
    public ProvisionTenantValidator(ITenantRepository repo)
    {
        RuleFor(x => x.Slug)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(63)
            .Matches(@"^[a-z0-9][a-z0-9\-]*[a-z0-9]$")
            .WithMessage("Slug deve conter apenas letras minúsculas, números e hífens. Não pode começar ou terminar com hífen.")
            .MustAsync(async (slug, ct) => !await repo.SlugExistsAsync(slug, ct))
            .WithMessage("Este slug já está em uso.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(200);
    }
}
