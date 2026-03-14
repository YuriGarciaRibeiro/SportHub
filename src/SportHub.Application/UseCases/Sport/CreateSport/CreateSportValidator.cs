using FluentValidation;

namespace Application.UseCases.Sport.CreateSport;

public class CreateSportValidator : AbstractValidator<CreateSportCommand>
{
    public CreateSportValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome do esporte é obrigatório.")
            .MaximumLength(100).WithMessage("Nome não pode exceder 100 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Descrição não pode exceder 500 caracteres.")
            .When(x => x.Description is not null);

        RuleFor(x => x.ImageUrl)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("URL da imagem inválida.")
            .When(x => !string.IsNullOrWhiteSpace(x.ImageUrl));
    }
}
