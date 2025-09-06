namespace Application.UseCases.Evaluation.EditEvaluation;

public class EditEvaluationValidator : AbstractValidator<EditEvaluationCommand>
{
    public EditEvaluationValidator()
    {
        RuleFor(x => x.EvaluationId).NotEmpty().WithMessage("EvaluationId is required.");
        RuleFor(x => x.Evaluation).NotNull().WithMessage("Evaluation data is required.");
        RuleFor(x => x.Evaluation.Rating)
            .InclusiveBetween(1, 5)
            .WithMessage("Rating must be between 1 and 5.");
        RuleFor(x => x.Evaluation.Comment)
            .MaximumLength(500)
            .WithMessage("Comment cannot exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.Evaluation.Comment));
    }
}
