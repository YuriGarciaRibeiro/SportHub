using FluentValidation;

namespace Application.UseCases.Court.UpdateCourt;

public class UpdateCourtValidator : AbstractValidator<UpdateCourtCommand>
{
    public UpdateCourtValidator()
    {
        RuleFor(x => x.Request.Name)
            .NotEmpty().WithMessage("Court name is required.")
            .When(x => x.Request.Name is not null);

        RuleFor(x => x.Request.SlotDurationMinutes)
            .GreaterThan(0).WithMessage("Slot duration must be positive.")
            .When(x => x.Request.SlotDurationMinutes.HasValue);

        RuleFor(x => x.Request.MinBookingSlots)
            .GreaterThan(0).WithMessage("Minimum booking slots must be positive.")
            .When(x => x.Request.MinBookingSlots.HasValue);

        RuleFor(x => x.Request.MaxBookingSlots)
            .GreaterThan(0).WithMessage("Maximum booking slots must be positive.")
            .When(x => x.Request.MaxBookingSlots.HasValue);

        RuleFor(x => x.Request.OpeningTime)
            .NotEmpty().WithMessage("Opening time is required.")
            .When(x => x.Request.OpeningTime.HasValue);

        RuleFor(x => x.Request.ClosingTime)
            .NotEmpty().WithMessage("Closing time is required.")
            .When(x => x.Request.ClosingTime.HasValue);

        RuleFor(x => x.Request.TimeZone)
            .NotEmpty().WithMessage("Time zone is required.")
            .When(x => x.Request.TimeZone is not null);
    }
}
