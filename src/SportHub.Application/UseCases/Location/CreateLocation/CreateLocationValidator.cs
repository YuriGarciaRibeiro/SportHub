using FluentValidation;

namespace Application.UseCases.Location.CreateLocation;

public class CreateLocationValidator : AbstractValidator<LocationRequest>
{
    public CreateLocationValidator()
    {
        RuleFor(l => l.Name)
            .NotEmpty().WithMessage("O nome da unidade é obrigatório.")
            .MaximumLength(100).WithMessage("O nome deve ter no máximo 100 caracteres.");

        When(l => l.Address is not null, () =>
        {
            RuleFor(l => l.Address!.Street).MaximumLength(200).When(l => l.Address!.Street is not null);
            RuleFor(l => l.Address!.Number).MaximumLength(20).When(l => l.Address!.Number is not null);
            RuleFor(l => l.Address!.Complement).MaximumLength(100).When(l => l.Address!.Complement is not null);
            RuleFor(l => l.Address!.Neighborhood).MaximumLength(100).When(l => l.Address!.Neighborhood is not null);
            RuleFor(l => l.Address!.City).MaximumLength(100).When(l => l.Address!.City is not null);
            RuleFor(l => l.Address!.State).MaximumLength(2).When(l => l.Address!.State is not null);
            RuleFor(l => l.Address!.ZipCode).MaximumLength(10).When(l => l.Address!.ZipCode is not null);
        });

        RuleFor(l => l.Phone)
            .MaximumLength(30).WithMessage("O telefone deve ter no máximo 30 caracteres.")
            .When(l => l.Phone is not null);

        RuleForEach(l => l.BusinessHours).ChildRules(schedule =>
        {
            schedule.RuleFor(s => s.DayOfWeek)
                .IsInEnum().WithMessage("Dia da semana inválido.");

            schedule.When(s => s.IsOpen, () =>
            {
                schedule.RuleFor(s => s.OpenTime)
                    .NotEmpty().WithMessage("Horário de abertura é obrigatório quando o dia está aberto.")
                    .Matches(@"^\d{2}:\d{2}$").WithMessage("Horário de abertura deve estar no formato HH:mm.");

                schedule.RuleFor(s => s.CloseTime)
                    .NotEmpty().WithMessage("Horário de fechamento é obrigatório quando o dia está aberto.")
                    .Matches(@"^\d{2}:\d{2}$").WithMessage("Horário de fechamento deve estar no formato HH:mm.");
            });
        });

        RuleFor(l => l.WhatsappNumber)
            .MaximumLength(20).WithMessage("O número do WhatsApp deve ter no máximo 20 caracteres.")
            .When(l => l.WhatsappNumber is not null);
    }
}
