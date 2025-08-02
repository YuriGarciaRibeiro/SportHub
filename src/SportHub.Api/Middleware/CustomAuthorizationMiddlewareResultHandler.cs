
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Mvc;

namespace Api.Middleware;

public class CustomAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler DefaultHandler = new();

    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        if (authorizeResult.Challenged)
        {
            var problemDetails = new ProblemDetails
            {
                Title = "Unauthorized",
                Detail = "Token missing or invalid.",
                Status = StatusCodes.Status401Unauthorized,
                Instance = context.Request.Path
            };

            problemDetails.Extensions.Add("traceId", context.TraceIdentifier);

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(problemDetails);
            return;
        }

        if (authorizeResult.Forbidden)
        {
            var problemDetails = new ProblemDetails
            {
                Title = "Forbidden",
                Detail = "You don't have permission to access this resource.",
                Status = StatusCodes.Status403Forbidden,
                Instance = context.Request.Path
            };

            problemDetails.Extensions.Add("traceId", context.TraceIdentifier);

            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(problemDetails);
            return;
        }

        await DefaultHandler.HandleAsync(next, context, policy, authorizeResult);
    }
}
