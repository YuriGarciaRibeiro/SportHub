
using Application.UseCases.Auth.Register;
using Application.UserCases.Auth;
using FluentResults;
using MediatR;
using WebApi.Extensions.ResultExtensions;

namespace SportHub.Api.Endpoints;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group =app.MapGroup("/auth")
            .WithTags("Auth");

        group.MapPost("/register", async (
            RegisterUserCommand command,
            ISender sender) =>
        {
            Result<AuthResponse> result = await sender.Send(command);

            return result.ToIResult();
        });

        group.MapPost("/login", async (
            LoginCommand command,
            ISender sender) =>
        {
            Result<AuthResponse> result = await sender.Send(command);

            return result.ToIResult();
        });

    
        return group;
    }
}
