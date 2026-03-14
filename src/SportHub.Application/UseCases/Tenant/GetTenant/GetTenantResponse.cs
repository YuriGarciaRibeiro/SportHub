namespace Application.UseCases.Tenant.GetTenant;

public record GetTenantResponse(
    Guid Id,
    string Slug,
    string Name,
    string Status,
    string? LogoUrl,
    string? PrimaryColor,
    string? CustomDomain,
    string? OwnerFirstName,
    string? OwnerLastName,
    string? OwnerEmail,
    DateTime CreatedAt
);
