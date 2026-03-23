using FluentValidation;

namespace Application.UseCases.Tenant.UpdateSettings;

public class UpdateSettingsValidator : AbstractValidator<UpdateSettingsCommand>
{
    public UpdateSettingsValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O nome de exibição é obrigatório.")
            .MaximumLength(100).WithMessage("O nome deve ter no máximo 100 caracteres.");

        RuleFor(x => x.PrimaryColor)
            .Matches("^#[0-9A-Fa-f]{6}$").WithMessage("A cor primária deve ser um hexadecimal válido (ex: #FF0000).")
            .When(x => !string.IsNullOrEmpty(x.PrimaryColor));

        RuleFor(x => x.LogoUrl)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _)).WithMessage("A URL da logo é inválida.")
            .When(x => !string.IsNullOrEmpty(x.LogoUrl));

        RuleFor(x => x.Tagline)
            .MaximumLength(150).WithMessage("A tagline deve ter no máximo 150 caracteres.")
            .When(x => !string.IsNullOrEmpty(x.Tagline));
    }
}
