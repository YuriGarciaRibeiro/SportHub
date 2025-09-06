using Domain.Enums;

namespace Application.UseCases.Evaluation.EditEvaluation;

public record EditEvaluationResponse
{
    public Guid EvaluationId { get; init; }
    public Guid UserId { get; init; }
    public Guid TargetId { get; init; }
    public EvaluationTargetType TargetType { get; init; }
    public int Rating { get; init; }
    public string? Comment { get; init; }
}