using System.Security.Claims;
using Application.Security;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace Infrastructure.Security;

public class SuperAdminHandler : AuthorizationHandler<SuperAdminRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        SuperAdminRequirement requirement)
    {
        var roleClaimValue = context.User.FindFirst(ClaimTypes.Role)?.Value;

        if (Enum.TryParse<UserRole>(roleClaimValue, out var role) && role == UserRole.SuperAdmin)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
