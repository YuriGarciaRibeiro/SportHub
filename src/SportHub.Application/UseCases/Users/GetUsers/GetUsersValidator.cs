using FluentValidation;

namespace SportHub.Application.UseCases.Users.GetUsers;

public class GetUsersValidator : AbstractValidator<GetUsersQuery>
{
    public GetUsersValidator()
    {
        RuleFor(x => x.Filter.Page)
            .GreaterThan(0)
            .WithMessage("Page deve ser maior que 0");

        RuleFor(x => x.Filter.PageSize)
            .GreaterThan(0)
            .WithMessage("PageSize deve ser maior que 0")
            .LessThanOrEqualTo(100)
            .WithMessage("PageSize não pode ser maior que 100");

        RuleFor(x => x.Filter.Email)
            .MaximumLength(255)
            .When(x => !string.IsNullOrWhiteSpace(x.Filter.Email))
            .WithMessage("Email deve ter no máximo 255 caracteres");

        RuleFor(x => x.Filter.FirstName)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Filter.FirstName))
            .WithMessage("FirstName deve ter no máximo 100 caracteres");

        RuleFor(x => x.Filter.LastName)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Filter.LastName))
            .WithMessage("LastName deve ter no máximo 100 caracteres");

        RuleFor(x => x.Filter.SearchTerm)
            .MaximumLength(255)
            .When(x => !string.IsNullOrWhiteSpace(x.Filter.SearchTerm))
            .WithMessage("SearchTerm deve ter no máximo 255 caracteres");
    }
}
