using FluentValidation;

public class CreateCourtValidator : AbstractValidator<CourtRequest>
{
    public CreateCourtValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("Court name is required.")
            .MaximumLength(100).WithMessage("Court name must not exceed 100 characters.");

        RuleFor(c => c.SportType)
            .NotEmpty().WithMessage("Sport type is required.")
            .MaximumLength(50).WithMessage("Sport type must not exceed 50 characters.");

        RuleFor(c => c.SlotDurationMinutes)
            .InclusiveBetween(30, 120).WithMessage("Slot duration must be between 15 and 120 minutes.");

        RuleFor(c => c.MinBookingSlots)
            .GreaterThan(0).WithMessage("Minimum booking slots must be greater than 0.");

        RuleFor(c => c.MaxBookingSlots)
            .GreaterThanOrEqualTo(c => c.MinBookingSlots).WithMessage("Maximum booking slots must be greater than or equal to minimum booking slots.");
    }
}
