using Domain.Enums;

namespace Application.UseCases.Evaluation.EditEvaluation;

public record EditEvaluationCommand : ICommand<EditEvaluationResponse>
{
    public Guid EvaluationId { get; init; }

    public EvaluationDto Evaluation { get; init; } = null!;
}

public record EvaluationDto
{
    public int? Rating { get; init; }
    public string? Comment { get; init; }
}
