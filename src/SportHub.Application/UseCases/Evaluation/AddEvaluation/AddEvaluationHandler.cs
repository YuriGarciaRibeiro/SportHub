

namespace Application.UseCases.Evalution.AddEvaluation;

public class AddEvaluationHandler : ICommandHandler<AddEvaluationCommand>
{
    private readonly IEvaluationService _evaluationService;
    private readonly IEvaluationRepository _evaluationRepository;
    private readonly IEstablishmentService _establishmentService;

        public AddEvaluationHandler(IEvaluationService evaluationService, IEvaluationRepository evaluationRepository, IEstablishmentService establishmentService)
        {
            _evaluationService = evaluationService;
            _evaluationRepository = evaluationRepository;
            _establishmentService = establishmentService;
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

        await _evaluationService.CreateAsync(evaluation, cancellationToken);

        if (evaluation.TargetType == Domain.Enums.EvaluationTargetType.Establishment)
        {
            await _establishmentService.InvalidateCacheAsync(evaluation.TargetId, cancellationToken);
        }

        return Result.Ok();
    }
}

