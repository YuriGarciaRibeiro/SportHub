using FluentValidation;

namespace Application.UseCases.Tenant.GetAllTenants;

public class GetAllTenantsValidator : AbstractValidator<GetAllTenantsQuery>
{
    public GetAllTenantsValidator()
    {
        RuleFor(x => x.Filter.Page)
            .GreaterThan(0)
            .WithMessage("Page deve ser maior que 0");

        RuleFor(x => x.Filter.PageSize)
            .GreaterThan(0)
            .WithMessage("PageSize deve ser maior que 0")
            .LessThanOrEqualTo(100)
            .WithMessage("PageSize não pode ser maior que 100");

        RuleFor(x => x.Filter.Name)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Filter.Name))
            .WithMessage("Name deve ter no máximo 200 caracteres");

        RuleFor(x => x.Filter.Slug)
            .MaximumLength(63)
            .When(x => !string.IsNullOrWhiteSpace(x.Filter.Slug))
            .WithMessage("Slug deve ter no máximo 63 caracteres");

        RuleFor(x => x.Filter.SearchTerm)
            .MaximumLength(255)
            .When(x => !string.IsNullOrWhiteSpace(x.Filter.SearchTerm))
            .WithMessage("SearchTerm deve ter no máximo 255 caracteres");
    }
}
