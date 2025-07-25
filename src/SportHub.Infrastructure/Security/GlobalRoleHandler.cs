using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Application.Common.Interfaces;
using Application.Security;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Security;

public class GlobalRoleHandler : AuthorizationHandler<GlobalRoleRequirement>
{
    private readonly IEstablishmentRoleService _svc;
    private readonly ILogger<GlobalRoleHandler> _logger;

    public GlobalRoleHandler(IEstablishmentRoleService svc, ILogger<GlobalRoleHandler> logger)
    {
        _svc = svc;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        GlobalRoleRequirement requirement)
    {
        _logger.LogInformation($"Checking global role: {requirement.RequiredRole}");

        var subClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
        if (subClaim is null || !Guid.TryParse(subClaim.Value, out var userId))
        {
            _logger.LogWarning("User ID not found in claims.");
            return;
        }

        var hasRole = await _svc.HasRoleAnywhereAsync(userId, requirement.RequiredRole);
        if (hasRole)
        {
            _logger.LogInformation($"User has global role: {requirement.RequiredRole}");
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogWarning($"User does not have global role: {requirement.RequiredRole}");
        }
    }
}