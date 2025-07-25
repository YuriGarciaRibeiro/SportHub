using Microsoft.AspNetCore.Authorization;
using Domain.Enums;

namespace Application.Security;

public class GlobalRoleRequirement : IAuthorizationRequirement
{
    public EstablishmentRole RequiredRole { get; }

    public GlobalRoleRequirement(EstablishmentRole requiredRole)
    {
        RequiredRole = requiredRole;
    }
}