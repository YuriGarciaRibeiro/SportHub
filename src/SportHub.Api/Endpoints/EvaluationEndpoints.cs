namespace Application.UseCases.Evaluation.AddEvaluation;

using Api.Extensions.Results;
using Application.UseCases.Evaluation.DeleteEvaluation;
using Application.UseCases.Evalution.AddEvaluation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

public static class EvaluationEndpoints
{
    public static void MapEvaluationEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/evaluations")
            .WithTags("Evaluations");

        group.MapPost("/", async (EvaluationDto evaluationDto, ISender sender, ICurrentUserService currentUserService) =>
        {
            var command = new AddEvaluationCommand
            {
                UserId = currentUserService.UserId,
                Evaluation = evaluationDto
            };

            var result = await sender.Send(command);
            return result.ToIResult(StatusCodes.Status201Created);
        })
        .WithName("AddEvaluation")
        .WithSummary("Add an evaluation")
        .WithDescription("Adds an evaluation for a target entity (e.g., establishment, event).")
        .Produces(StatusCodes.Status201Created)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
        .RequireAuthorization();

        group.MapDelete("/{EvaluationId}", async (Guid evaluationId, ISender sender, ICurrentUserService currentUserService) =>
        {
            var command = new DeleteEvaluationCommand
            {
                EvaluationId = evaluationId
            };

            var result = await sender.Send(command);
            return result.ToIResult();
        })
        .WithName("DeleteEvaluation")
        .WithSummary("Delete an evaluation")
        .WithDescription("Deletes an evaluation for a target entity (e.g., establishment, event).")
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
        .RequireAuthorization();

        // PUT /evaluations/{EvaluationId} - Update an evaluation
        group.MapPut("/{EvaluationId}", async (Guid evaluationId, EditEvaluation.EvaluationDto evaluationDto, ISender sender, ICurrentUserService currentUserService) =>
        {
            var command = new EditEvaluation.EditEvaluationCommand
            {
                EvaluationId = evaluationId,
                Evaluation = evaluationDto
            };

            var result = await sender.Send(command);
            return result.ToIResult();
        })
        .WithName("EditEvaluation")
        .WithSummary("Edit an evaluation")
        .WithDescription("Edits an existing evaluation for a target entity (e.g., establishment, event).")
        .Produces<EditEvaluation.EditEvaluationResponse>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
        .RequireAuthorization();
    }
}