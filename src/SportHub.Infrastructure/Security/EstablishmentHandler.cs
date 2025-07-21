using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Application.Common.Interfaces;
using Application.Security;
using Domain.Enums;

namespace Infrastructure.Security;

public class EstablishmentHandler
  : AuthorizationHandler<EstablishmentRequirement, HttpContext>
{
    private readonly IEstablishmentRoleService _svc;
    public EstablishmentHandler(IEstablishmentRoleService svc)
      => _svc = svc;

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        EstablishmentRequirement requirement,
        HttpContext httpContext)
    {
        // pega o establishmentId da rota
        var raw = httpContext.GetRouteValue("establishmentId");
        if (raw is null || !Guid.TryParse(raw.ToString(), out var estId))
            return;

        // pega userId do token
        var sub = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (sub is null || !Guid.TryParse(sub, out var userId))
            return;

        // checa hierarquia
        if (await _svc.HasAtLeastRoleAsync(userId, estId, requirement.RequiredRole))
            context.Succeed(requirement);
    }
}
