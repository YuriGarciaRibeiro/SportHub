using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Api.Middleware;

public class CustomExceptionHandler(ILogger<CustomExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Error occurred at {Time}", DateTime.UtcNow);

        (string Detail, string Title, int StatusCode) details = exception switch
        {
            ValidationException => (
                exception.Message,
                "Validation Failed",
                StatusCodes.Status422UnprocessableEntity
            ),
            DbUpdateException dbEx when dbEx.InnerException is PostgresException pgEx &&
                pgEx.SqlState == PostgresErrorCodes.UniqueViolation => (
                $"Unique constraint violated: {pgEx.ConstraintName}.",
                "Unique Constraint Violation",
                StatusCodes.Status400BadRequest
            ),
            _ => (
                exception.Message,
                "Internal Server Error",
                StatusCodes.Status500InternalServerError
            )
        };

        var problemDetails = new ProblemDetails
        {
            Title = details.Title,
            Detail = details.Detail,
            Status = details.StatusCode,
            Instance = context.Request.Path
        };

        problemDetails.Extensions.Add("traceId", context.TraceIdentifier);

        if (exception is ValidationException validationException)
        {
            problemDetails.Extensions.Add("errors", validationException.Errors);
        }

        context.Response.StatusCode = details.StatusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken: cancellationToken);
        return true;
    }
}
