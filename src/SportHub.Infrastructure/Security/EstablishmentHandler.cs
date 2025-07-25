using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Application.Common.Interfaces;
using Application.Security;
using Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Security;

public class EstablishmentHandler
  : AuthorizationHandler<EstablishmentRequirement, HttpContext>
{
    private readonly IEstablishmentRoleService _svc;
    private readonly ILogger<EstablishmentHandler> _logger;

    public EstablishmentHandler(IEstablishmentRoleService svc, ILogger<EstablishmentHandler> logger)
    {
        _svc = svc;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        EstablishmentRequirement requirement,
        HttpContext httpContext)
    {
        _logger.LogInformation($"Accessing route: {httpContext.Request.Path}");
        _logger.LogInformation($"Checking establishment role: {requirement.RequiredRole}");

        var raw = httpContext.GetRouteValue("establishmentId");
        if (raw is null || !Guid.TryParse(raw.ToString(), out var estId))
        {
            _logger.LogWarning("Establishment ID not provided or invalid.");
            return;
        }

        var subClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
        if (subClaim is null || !Guid.TryParse(subClaim.Value, out var userId))
        {
            _logger.LogWarning("User ID not found in claims.");
            return;
        }

        if (await _svc.HasAtLeastRoleAsync(userId, estId, requirement.RequiredRole))
        {
            _logger.LogInformation($"User has role {requirement.RequiredRole} in establishment {estId}");
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogWarning($"User does not have role {requirement.RequiredRole} in establishment {estId}");
        }
    }
}
