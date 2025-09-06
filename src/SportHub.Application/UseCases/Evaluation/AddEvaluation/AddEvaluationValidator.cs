namespace Application.UseCases.Evalution.AddEvaluation;

public class AddEvaluationValidator : AbstractValidator<AddEvaluationCommand>
{
    public AddEvaluationValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.Evaluation.TargetId)
            .NotEmpty().WithMessage("Target ID is required.");

        RuleFor(x => x.Evaluation.TargetType)
            .IsInEnum().WithMessage("Invalid target type.");

        RuleFor(x => x.Evaluation.Rating)
            .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5.");

        RuleFor(x => x.Evaluation.Comment)
            .MaximumLength(500).WithMessage("Comment must not exceed 500 characters.");
    }
}
