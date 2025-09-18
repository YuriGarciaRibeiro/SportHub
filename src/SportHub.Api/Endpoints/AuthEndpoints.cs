using Application.UseCases.Auth.Register;
using Application.UseCases.Auth;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Api.Extensions.Results;
using Application.UseCases.Auth.Login;
using Application.UseCases.Auth.ForgotPassword;
using Microsoft.AspNetCore.Diagnostics;
using Application.Common.Interfaces.Email;
using Application.UseCases.Auth.VerifyForgotPasswordCode;
using Application.UseCases.Auth.UpdatePassword;

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

        group.MapPost("/forgot-password", async (
            ForgotPasswordCommand command,
            ISender sender) =>
        {
            Result result = await sender.Send(command);

            return result.ToIResult(StatusCodes.Status202Accepted);
        })
        .WithName("ForgotPassword")
        .WithSummary("Request password reset")
        .WithDescription("Sends a password reset email to the user.")
        .Produces(StatusCodes.Status202Accepted)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        group.MapPost("forgot-password/verify", async (
            VerifyForgotPasswordCodeCommand command,
            ISender sender) =>
        {
            var result = await sender.Send(command);

            return result.ToIResult(StatusCodes.Status200OK);
        })
        .WithName("VerifyForgotPasswordCode")
        .WithSummary("Verify forgot password code")
        .WithDescription("Verifies the code sent to the user's email for password reset.")
        .Produces<VerifyForgotPasswordCodeResponse>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        group.MapPost("forgot-password/update", async (
            UpdatePasswordCommand command,
            ISender sender) =>
        {
            var result = await sender.Send(command);

            return result.ToIResult(StatusCodes.Status200OK);
        })
        .WithName("UpdatePassword")
        .WithSummary("Update password")
        .WithDescription("Updates the user's password using a valid reset session.")
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        return group;
    }
}
