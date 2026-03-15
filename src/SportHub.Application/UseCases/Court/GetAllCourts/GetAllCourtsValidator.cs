using FluentValidation;

namespace Application.UseCases.Court.GetAllCourts;

public class GetAllCourtsValidator : AbstractValidator<GetAllCourtsQuery>
{
    public GetAllCourtsValidator()
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
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Filter.Name))
            .WithMessage("Name deve ter no máximo 100 caracteres");

        RuleFor(x => x.Filter.MinPrice)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Filter.MinPrice.HasValue)
            .WithMessage("MinPrice deve ser maior ou igual a 0");

        RuleFor(x => x.Filter.MaxPrice)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Filter.MaxPrice.HasValue)
            .WithMessage("MaxPrice deve ser maior ou igual a 0");

        RuleFor(x => x.Filter)
            .Must(f => !f.MinPrice.HasValue || !f.MaxPrice.HasValue || f.MinPrice <= f.MaxPrice)
            .WithMessage("MinPrice deve ser menor ou igual a MaxPrice");

        RuleFor(x => x.Filter.SearchTerm)
            .MaximumLength(255)
            .When(x => !string.IsNullOrWhiteSpace(x.Filter.SearchTerm))
            .WithMessage("SearchTerm deve ter no máximo 255 caracteres");
    }
}
