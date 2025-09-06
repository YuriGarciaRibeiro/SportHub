
namespace Application.UseCases.Evaluation.DeleteEvaluation;

public class DeleteEvaluationHandler : ICommandHandler<DeleteEvaluationCommand>
{
    private readonly IEvaluationService _evaluationService;
    private readonly ICurrentUserService _currentUserService;

    public DeleteEvaluationHandler(IEvaluationService evaluationService, ICurrentUserService currentUserService)
    {
        _evaluationService = evaluationService;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(DeleteEvaluationCommand request, CancellationToken cancellationToken)
    {
        var evaluation = await _evaluationService.GetByIdAsync(request.EvaluationId, cancellationToken);
        var userId = _currentUserService.UserId;
        if (evaluation == null)
        {
            return Result.Fail("Evaluation not found.");
        }

        if (evaluation.UserId != userId)
        {
            return Result.Fail("Evaluation not found for this user.");
        }

        await _evaluationService.DeleteAsync(evaluation, cancellationToken);

        return Result.Ok();
    }
}