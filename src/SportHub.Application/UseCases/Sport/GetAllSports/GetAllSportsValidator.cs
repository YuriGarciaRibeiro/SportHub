using FluentValidation;

namespace Application.UseCases.Sport.GetAllSports;

public class GetAllSportsValidator : AbstractValidator<GetAllSportsQuery>
{
    public GetAllSportsValidator()
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

        RuleFor(x => x.Filter.SearchTerm)
            .MaximumLength(255)
            .When(x => !string.IsNullOrWhiteSpace(x.Filter.SearchTerm))
            .WithMessage("SearchTerm deve ter no máximo 255 caracteres");
    }
}
