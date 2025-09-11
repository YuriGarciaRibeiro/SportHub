using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Extensions.Results;

public static class ResultExtensions
{
    private static IHttpContextAccessor? _httpContextAccessor;

    public static void Configure(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public static IResult ToIResult<T>(this Result<T> result, int? successStatusCode = null)
    {
        if (result.IsSuccess)
        {
            var statusCode = successStatusCode ?? StatusCodes.Status200OK;
            return Microsoft.AspNetCore.Http.Results.Json(result.Value, statusCode: statusCode);
        }

        return BuildProblemResult(result.Errors);
    }

    public static IResult ToIResult(this Result result, int? successStatusCode = null)
    {
        if (result.IsSuccess)
            return successStatusCode.HasValue
                ? Microsoft.AspNetCore.Http.Results.Json(null, statusCode: successStatusCode.Value)
                : Microsoft.AspNetCore.Http.Results.Ok();

        return BuildProblemResult(result.Errors);
    }

    private static IResult BuildProblemResult(IReadOnlyList<IError> errors)
    {
        var context = _httpContextAccessor?.HttpContext;
        var path = context?.Request?.Path.Value ?? "";
        var traceId = context?.TraceIdentifier;

        var firstError = errors.FirstOrDefault();
        var statusCode = GetErrorStatusCode(firstError);

        var problem = CreateProblemDetails(errors, statusCode, path, traceId);

        return Microsoft.AspNetCore.Http.Results.Problem(
            problem.Detail, 
            statusCode: statusCode, 
            title: problem.Title, 
            extensions: problem.Extensions
        );
    }

    private static int GetErrorStatusCode(IError? error)
    {
        return error?.Metadata.TryGetValue("StatusCode", out var codeObj) == true && codeObj is int code
            ? code
            : StatusCodes.Status400BadRequest;
    }

    private static ProblemDetails CreateProblemDetails(IReadOnlyList<IError> errors, int statusCode, string path, string? traceId)
    {
        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = "Operation failed",
            Detail = string.Join("; ", errors.Select(e => e.Message)),
            Instance = path
        };

        if (!string.IsNullOrWhiteSpace(traceId))
        {
            problem.Extensions["traceId"] = traceId;
        }

        problem.Extensions["errors"] = errors.Select(e => new
        {
            e.Message,
            Metadata = e.Metadata
        }).ToList();

        return problem;
    }
}
