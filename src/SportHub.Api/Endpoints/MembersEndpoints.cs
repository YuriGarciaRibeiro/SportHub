using Application.Common.Models;
using Application.Security;
using Application.UseCases.Members.GetMembers;
using Application.UseCases.Members.UpdateMemberRole;
using Application.UseCases.Members.UpsertMember;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebApi.Extensions.ResultExtensions;

namespace SportHub.Api.Endpoints;

public static class MembersEndpoints
{
    public static void MapMembersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/members")
            .WithTags("Members")
            .RequireAuthorization(PolicyNames.IsOwner);

        // GET /api/members — Lista membros operacionais (Staff+)
        group.MapGet("/", async (
            [AsParameters] GetMembersFilter filter,
            ISender sender) =>
        {
            var result = await sender.Send(new GetMembersQuery(filter));
            return result.ToIResult();
        })
        .WithName("GetMembers")
        .WithSummary("Lista todos os membros operacionais do tenant (Staff, Manager, Owner)")
        .Produces<PagedResult<MemberDto>>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
        .Produces<ProblemDetails>(StatusCodes.Status403Forbidden);

        // POST /api/members — Cria ou atualiza role de membro
        group.MapPost("/", async (UpsertMemberRequest request, ISender sender) =>
        {
            if (!Enum.TryParse<UserRole>(request.Role, ignoreCase: true, out var role))
                return Results.Problem("Invalid role value.", statusCode: StatusCodes.Status422UnprocessableEntity);

            var command = new UpsertMemberCommand(request.Email, request.FirstName, request.LastName, role);
            var result = await sender.Send(command);
            return result.ToIResult();
        })
        .WithName("UpsertMember")
        .WithSummary("Cria novo membro com senha padrão ou atualiza o role de um membro existente")
        .Produces<MemberDto>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
        .Produces<ProblemDetails>(StatusCodes.Status403Forbidden);

        // PUT /api/members/{id}/role — Atualiza role de um membro
        group.MapPut("/{id:guid}/role", async (Guid id, UpdateMemberRoleRequest request, ISender sender) =>
        {
            if (!Enum.TryParse<UserRole>(request.Role, ignoreCase: true, out var role))
                return Results.Problem("Invalid role value.", statusCode: StatusCodes.Status422UnprocessableEntity);

            var command = new UpdateMemberRoleCommand(id, role);
            var result = await sender.Send(command);
            return result.ToIResult();
        })
        .WithName("UpdateMemberRole")
        .WithSummary("Atualiza o role de um membro (apenas Owner)")
        .Produces<MemberRoleUpdatedResponse>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }
}

public record UpdateMemberRoleRequest(string Role);
public record UpsertMemberRequest(string Email, string FirstName, string LastName, string Role);
