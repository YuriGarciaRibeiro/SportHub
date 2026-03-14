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
    }
}
