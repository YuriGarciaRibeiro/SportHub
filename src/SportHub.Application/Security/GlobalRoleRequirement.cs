using Microsoft.AspNetCore.Authorization;
using Domain.Enums;

namespace Application.Security;

public class GlobalRoleRequirement : IAuthorizationRequirement
{
    public UserRole RequiredRole { get; }

    public GlobalRoleRequirement(UserRole requiredRole)
    {
        RequiredRole = requiredRole;
    }
}