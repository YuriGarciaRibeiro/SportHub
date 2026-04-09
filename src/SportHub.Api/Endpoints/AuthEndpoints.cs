using Application.UseCases.Auth.Register;
using Application.UseCases.Auth;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebApi.Extensions.ResultExtensions;
using Application.UseCases.Auth.Login;
using Application.UseCases.Auth.RefreshToken;
using Application.UseCases.Auth.GetCurrentUser;
using Application.UseCases.Auth.UpdateCurrentUser;
using Application.UseCases.Auth.ChangePassword;
using Application.UseCases.Auth.ForgotPassword;
using Application.UseCases.Auth.ResetPassword;
using SportHub.Application.UseCases.Auth.DeleteUser;
using Application.Common.Interfaces;

namespace SportHub.Api.Endpoints;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth")
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

        group.MapPost("/refresh", async (
            RefreshTokenCommand command,
            ISender sender) =>
        {
            Result<AuthResponse> result = await sender.Send(command);
            return result.ToIResult();
        })
        .WithName("RefreshToken")
        .WithSummary("Refresh access token using a valid refresh token")
        .AllowAnonymous()
        .Produces<AuthResponse>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
        .Produces<ProblemDetails>(StatusCodes.Status422UnprocessableEntity);

        // GET /auth/me — perfil do usuário logado
        group.MapGet("/me", async (ISender sender) =>
        {
            var result = await sender.Send(new GetCurrentUserQuery());
            return result.ToIResult();
        })
        .WithName("GetCurrentUser")
        .WithSummary("Retorna o perfil do usuário autenticado")
        .RequireAuthorization()
        .Produces<UserProfileResponse>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        // PUT /auth/me — atualizar nome do usuário logado
        group.MapPut("/me", async (UpdateCurrentUserCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.ToIResult();
        })
        .WithName("UpdateCurrentUser")
        .WithSummary("Atualiza o nome do usuário autenticado")
        .RequireAuthorization()
        .Produces<UserProfileResponse>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status422UnprocessableEntity);

        // DELETE /auth/me — deletar própria conta (soft delete)
        group.MapDelete("/me", async (ISender sender, ICurrentUserService currentUserService) =>
        {
            var result = await sender.Send(new DeleteUserCommand(currentUserService.UserId));
            return result.ToIResult();
        })
        .WithName("DeleteCurrentUser")
        .WithSummary("Deleta (soft) a conta do usuário autenticado")
        .RequireAuthorization()
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        // PUT /auth/change-password — alterar senha estando logado
        group.MapPut("/change-password", async (ChangePasswordCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.ToIResult();
        })
        .WithName("ChangePassword")
        .WithSummary("Altera a senha do usuário autenticado")
        .RequireAuthorization()
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
        .Produces<ProblemDetails>(StatusCodes.Status422UnprocessableEntity);

        // POST /auth/forgot-password — solicitar reset de senha
        group.MapPost("/forgot-password", async (ForgotPasswordCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.ToIResult();
        })
        .WithName("ForgotPassword")
        .WithSummary("Solicita um token de reset de senha (enviado por email)")
        .AllowAnonymous()
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status422UnprocessableEntity);

        // POST /auth/reset-password — resetar senha com token
        group.MapPost("/reset-password", async (ResetPasswordCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.ToIResult();
        })
        .WithName("ResetPassword")
        .WithSummary("Reseta a senha usando um token válido")
        .AllowAnonymous()
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status422UnprocessableEntity);

        return group;
    }
}
