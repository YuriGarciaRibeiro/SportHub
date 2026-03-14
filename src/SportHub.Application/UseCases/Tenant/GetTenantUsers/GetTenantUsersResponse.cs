namespace Application.UseCases.Tenant.GetTenantUsers;

public record GetTenantUsersResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string Role
);
