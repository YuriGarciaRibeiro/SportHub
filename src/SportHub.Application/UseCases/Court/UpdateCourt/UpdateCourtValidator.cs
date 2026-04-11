using Application.UseCases.Court.CreateCourt;
using FluentValidation;

namespace Application.UseCases.Court.UpdateCourt;

public class UpdateCourtValidator : AbstractValidator<UpdateCourtCommand>
{
    public UpdateCourtValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("O ID da quadra é obrigatório.");

        RuleFor(x => x.Court.Name)
            .NotEmpty().WithMessage("O nome da quadra é obrigatório.")
            .MaximumLength(100);

        RuleFor(x => x.Court.PricePerHour)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Court.SlotDurationMinutes)
            .InclusiveBetween(30, 120);

        RuleFor(x => x.Court.MinBookingSlots)
            .GreaterThan(0);

        RuleFor(x => x.Court.MaxBookingSlots)
            .GreaterThanOrEqualTo(x => x.Court.MinBookingSlots);

        RuleFor(x => x.Court.Sports)
            .NotEmpty().WithMessage("A quadra deve ter pelo menos um esporte vinculado.");
        
        RuleFor(x => x.Court.PeakPricePerHour)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Court.PeakPricePerHour.HasValue)
            .WithMessage("O preço de pico por hora deve ser maior ou igual a zero.");

        RuleFor(x => x.Court.CancelationWindowHours)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Court.CancelationWindowHours.HasValue)
            .WithMessage("A janela de cancelamento deve ser maior ou igual a zero.");

        RuleFor(x => x.Court.LateCancellationFeePercent)
            .GreaterThanOrEqualTo(0).LessThanOrEqualTo(100)
            .When(x => x.Court.LateCancellationFeePercent.HasValue)
            .WithMessage("A taxa de cancelamento tardio deve ser entre 0 e 100.");
    }
}
