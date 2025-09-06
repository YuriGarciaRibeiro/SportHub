

namespace Application.UseCases.Evalution.AddEvaluation;

public class AddEvaluationHandler : ICommandHandler<AddEvaluationCommand>
{
    private readonly IEvaluationRepository _evaluationRepository;

    public AddEvaluationHandler(IEvaluationRepository evaluationRepository)
    {
        _evaluationRepository = evaluationRepository;
    }

    public async Task<Result> Handle(AddEvaluationCommand request, CancellationToken cancellationToken)
    {   
        var evaluationDto = request.Evaluation;

        var existingEvaluation = await _evaluationRepository.HasUserEvaluatedTargetAsync(request.UserId, evaluationDto.TargetId, evaluationDto.TargetType, cancellationToken);

        if (existingEvaluation) return Result.Fail("User has already evaluated this target.");

        var evaluation = new Domain.Entities.Evaluation
        {
            UserId = request.UserId,
            TargetId = evaluationDto.TargetId,
            TargetType = evaluationDto.TargetType,
            Rating = evaluationDto.Rating,
            Comment = evaluationDto.Comment
        };

        await _evaluationRepository.AddAsync(evaluation, cancellationToken);

        return Result.Ok();
    }
}

