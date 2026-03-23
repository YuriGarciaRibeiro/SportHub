using Application.UseCases.Location.CreateLocation;
using FluentValidation;

namespace Application.UseCases.Location.UpdateLocation;

public class UpdateLocationValidator : AbstractValidator<UpdateLocationCommand>
{
    public UpdateLocationValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("O ID da unidade é obrigatório.");

        RuleFor(x => x.Location.Name)
            .NotEmpty().WithMessage("O nome da unidade é obrigatório.")
            .MaximumLength(100).WithMessage("O nome deve ter no máximo 100 caracteres.");

        When(x => x.Location.Address is not null, () =>
        {
            RuleFor(x => x.Location.Address!.Street).MaximumLength(200).When(x => x.Location.Address!.Street is not null);
            RuleFor(x => x.Location.Address!.Number).MaximumLength(20).When(x => x.Location.Address!.Number is not null);
            RuleFor(x => x.Location.Address!.Complement).MaximumLength(100).When(x => x.Location.Address!.Complement is not null);
            RuleFor(x => x.Location.Address!.Neighborhood).MaximumLength(100).When(x => x.Location.Address!.Neighborhood is not null);
            RuleFor(x => x.Location.Address!.City).MaximumLength(100).When(x => x.Location.Address!.City is not null);
            RuleFor(x => x.Location.Address!.State).MaximumLength(2).When(x => x.Location.Address!.State is not null);
            RuleFor(x => x.Location.Address!.ZipCode).MaximumLength(10).When(x => x.Location.Address!.ZipCode is not null);
        });

        RuleFor(x => x.Location.Phone)
            .MaximumLength(30).WithMessage("O telefone deve ter no máximo 30 caracteres.")
            .When(x => x.Location.Phone is not null);

        RuleForEach(x => x.Location.BusinessHours).ChildRules(schedule =>
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

        RuleFor(x => x.Location.WhatsappNumber)
            .MaximumLength(20).WithMessage("O número do WhatsApp deve ter no máximo 20 caracteres.")
            .When(x => x.Location.WhatsappNumber is not null);
    }
}
