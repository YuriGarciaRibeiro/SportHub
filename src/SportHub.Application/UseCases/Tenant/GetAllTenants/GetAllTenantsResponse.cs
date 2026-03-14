namespace Application.UseCases.Tenant.GetAllTenants;

public record GetAllTenantsResponse(
    Guid Id,
    string Slug,
    string Name,
    string Status,
    string? LogoUrl,
    string? PrimaryColor,
    string? OwnerFirstName,
    string? OwnerLastName,
    string? OwnerEmail,
    DateTime CreatedAt
);
