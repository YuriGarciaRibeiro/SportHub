using FluentValidation;

namespace Application.UseCases.Court.CreateCourt;


public class CreateCourtValidator : AbstractValidator<CourtRequest>
{
    public CreateCourtValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("Court name is required.")
            .MaximumLength(100).WithMessage("Court name must not exceed 100 characters.");

        RuleFor(c => c.PricePerHour)
            .GreaterThanOrEqualTo(0).WithMessage("Price per hour must be 0 or greater.");

        RuleFor(c => c.SlotDurationMinutes)
            .InclusiveBetween(30, 120).WithMessage("Slot duration must be between 30 and 120 minutes.");

        RuleFor(c => c.MinBookingSlots)
            .GreaterThan(0).WithMessage("Minimum booking slots must be greater than 0.");

        RuleFor(c => c.MaxBookingSlots)
            .GreaterThanOrEqualTo(c => c.MinBookingSlots).WithMessage("Maximum booking slots must be greater than or equal to minimum booking slots.");

        RuleFor(c => c.Sports)
            .NotEmpty().WithMessage("A quadra deve ter pelo menos um esporte vinculado.");

        RuleFor(c => c.PeakPricePerHour)
            .GreaterThanOrEqualTo(0)
            .When(c => c.PeakPricePerHour.HasValue)
            .WithMessage("O preço de pico por hora deve ser maior ou igual a zero.");

        RuleFor(c => c.CancelationWindowHours)
            .GreaterThanOrEqualTo(0)
            .When(c => c.CancelationWindowHours.HasValue)
            .WithMessage("A janela de cancelamento deve ser maior ou igual a zero.");

        RuleFor(c => c.LateCancellationFeePercent)
            .GreaterThanOrEqualTo(0).LessThanOrEqualTo(100)
            .When(c => c.LateCancellationFeePercent.HasValue)
            .WithMessage("A taxa de cancelamento tardio deve ser entre 0 e 100.");
    }
}
