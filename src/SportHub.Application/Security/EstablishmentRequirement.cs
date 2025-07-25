using Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace Application.Security;
public class EstablishmentRequirement : IAuthorizationRequirement
{
    public EstablishmentRole RequiredRole { get; }

    public EstablishmentRequirement(EstablishmentRole role)
        => RequiredRole = role;
}