using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Application.Security;
using Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Security;

public class GlobalRoleHandler : AuthorizationHandler<GlobalRoleRequirement>
{
    private readonly ILogger<GlobalRoleHandler> _logger;

    public GlobalRoleHandler(ILogger<GlobalRoleHandler> logger)
    {
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        GlobalRoleRequirement requirement)
    {
        _logger.LogInformation("Checking global role: {RequiredRole}", requirement.RequiredRole);

        // The user's role is stored in the JWT claim as a string (e.g. "Owner", "Manager", "Staff")
        var roleClaim = context.User.FindFirst(ClaimTypes.Role)?.Value;

        if (roleClaim is null)
        {
            _logger.LogWarning("Role claim not found.");
            return Task.CompletedTask;
        }

        if (Enum.TryParse<EstablishmentRole>(roleClaim, ignoreCase: true, out var userRole)
            && userRole >= requirement.RequiredRole)
        {
            _logger.LogInformation("User has required role {RequiredRole} (has {UserRole})",
                requirement.RequiredRole, userRole);
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogWarning("User with role {UserRole} does not satisfy {RequiredRole}",
                roleClaim, requirement.RequiredRole);
        }

        return Task.CompletedTask;
    }
}