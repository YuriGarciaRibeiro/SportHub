using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace SportHub.Api.Middleware;

public class CustomAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler DefaultHandler = new();

    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        if (authorizeResult.Challenged) // 401
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            var payload = JsonSerializer.Serialize(new
            {
                message = "Token faltando ou inválido."
            });
            await context.Response.WriteAsync(payload);
            return;
        }

        if (authorizeResult.Forbidden)  // 403
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            var payload = JsonSerializer.Serialize(new
            {
                message = "You don't have permission to access this resource."
            });
            await context.Response.WriteAsync(payload);
            return;
        }

        await DefaultHandler.HandleAsync(next, context, policy, authorizeResult);
    }
}
