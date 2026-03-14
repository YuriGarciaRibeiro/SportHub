using Application.Security;
using Application.UseCases.Members.GetMembers;
using Application.UseCases.Members.UpdateMemberRole;
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
        group.MapGet("/", async (ISender sender) =>
        {
            var result = await sender.Send(new GetMembersQuery());
            return result.ToIResult();
        })
        .WithName("GetMembers")
        .WithSummary("Lista todos os membros operacionais do tenant (Staff, Manager, Owner)")
        .Produces<List<MemberDto>>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
        .Produces<ProblemDetails>(StatusCodes.Status403Forbidden);

        // PUT /api/members/{id}/role — Atualiza role de um membro
        group.MapPut("/{id:guid}/role", async (Guid id, UpdateMemberRoleRequest request, ISender sender) =>
        {
            var command = new UpdateMemberRoleCommand(id, request.NewRole);
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

public record UpdateMemberRoleRequest(UserRole NewRole);
