
using Application.Common.Errors;

namespace Application.UseCases.Evaluation.EditEvaluation;

public class EditEvaluationHandler : ICommandHandler<EditEvaluationCommand, EditEvaluationResponse>
{
    public readonly IEvaluationService _evaluationService;
    public readonly ICurrentUserService _currentUserService;

    public EditEvaluationHandler(IEvaluationService evaluationService, ICurrentUserService currentUserService)
    {
        _evaluationService = evaluationService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<EditEvaluationResponse>> Handle(EditEvaluationCommand request, CancellationToken cancellationToken)
    {
        var evaluation = await _evaluationService.GetByIdAsync(request.EvaluationId, cancellationToken);
        var userId = _currentUserService.UserId;
        if (evaluation == null)
        {
            return Result.Fail(new NotFound("Evaluation not found."));
        }

        if (evaluation.UserId != userId)
        {
            return Result.Fail(new NotFound("Evaluation not found for this user."));
        }

        var evaluationDto = request.Evaluation;

        evaluation.Rating = evaluationDto.Rating ?? evaluation.Rating;
        evaluation.Comment = evaluationDto.Comment ?? evaluation.Comment;
        
        await _evaluationService.UpdateAsync(evaluation, cancellationToken);

        var response = new EditEvaluationResponse
        {
            EvaluationId = evaluation.Id,
            UserId = evaluation.UserId,
            TargetId = evaluation.TargetId,
            TargetType = evaluation.TargetType,
            Rating = evaluation.Rating,
            Comment = evaluation.Comment
        };
        return Result.Ok(response);
    }
}