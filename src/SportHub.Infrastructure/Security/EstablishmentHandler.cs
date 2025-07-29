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
    private readonly IUserService _userService;

    public EstablishmentHandler(IEstablishmentRoleService svc, ILogger<EstablishmentHandler> logger, IUserService userService)
    {
        _svc = svc;
        _logger = logger;
        _userService = userService;
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

        var user = await _userService.GetUserByIdAsync(userId);
        if (user is null)
        {
            _logger.LogWarning($"User with ID {userId} not found.");
            return;
        }

        if (user.Value.Role == UserRole.Admin)
        {
            _logger.LogInformation("User is an admin, skipping establishment role check.");
            context.Succeed(requirement);
            return;
        }

        if (user.Value.Role == UserRole.User)
        {
            _logger.LogInformation("User is a regular user, skipping establishment role check.");
            context.Succeed(requirement);
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
