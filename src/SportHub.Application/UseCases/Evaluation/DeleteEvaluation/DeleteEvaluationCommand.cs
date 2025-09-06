namespace Application.UseCases.Evaluation.DeleteEvaluation;

public record DeleteEvaluationCommand : ICommand
{
    public Guid EvaluationId { get; init; }
}