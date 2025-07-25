
using Application.Common.Interfaces;
using Application.Security;
using Application.UseCases.Auth.Register;
using Application.UserCases.Auth;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Mvc;
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
        })
        .WithName("RegisterUser")
        .WithSummary("Register a new user")
        .WithDescription("Registers a new user with the provided details.")
        .Produces<AuthResponse>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status409Conflict)
        .Produces<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        group.MapPost("/login", async (
            LoginCommand command,
            ISender sender) =>
        {
            Result<AuthResponse> result = await sender.Send(command);

            return result.ToIResult();
        })
        .WithName("LoginUser")
        .WithSummary("Login a user")
        .WithDescription("Logs in a user with the provided email and password.")
        .Produces<AuthResponse>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
        .Produces<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        return group;
    }
}
