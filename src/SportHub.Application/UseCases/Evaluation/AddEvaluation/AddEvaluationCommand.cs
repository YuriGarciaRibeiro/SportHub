using Domain.Enums;

namespace Application.UseCases.Evalution.AddEvaluation;

public record AddEvaluationCommand : ICommand
{
    public Guid UserId { get; init; }

    public EvaluationDto Evaluation { get; init; } = null!;
}

public record EvaluationDto
{
    public Guid TargetId { get; init; }
    public EvaluationTargetType TargetType { get; set; } 
    public int Rating { get; init; }
    public string Comment { get; init; } = null!;
}