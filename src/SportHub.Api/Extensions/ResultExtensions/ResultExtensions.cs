using FluentResults;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Extensions.ResultExtensions;

public static class ResultExtensions
{
    public static IResult ToIResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
            return Results.Ok(result.Value);

        var firstError = result.Errors.FirstOrDefault();
        var statusCode = firstError?.Metadata.TryGetValue("StatusCode", out var codeObj) == true && codeObj is int code
            ? code
            : 400;

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = "Operation failed",
            Detail = string.Join("; ", result.Errors.Select(e => e.Message))
        };

        return Results.Problem(problem.Detail, statusCode: statusCode, title: problem.Title);
    }

    public static IResult ToIResult(this Result result)
    {
        if (result.IsSuccess)
            return Results.NoContent();

        var firstError = result.Errors.FirstOrDefault();
        var statusCode = firstError?.Metadata.TryGetValue("StatusCode", out var codeObj) == true && codeObj is int code
            ? code
            : 400;

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = "Operation failed",
            Detail = string.Join("; ", result.Errors.Select(e => e.Message))
        };

        return Results.Problem(problem.Detail, statusCode: statusCode, title: problem.Title);
    }
}
