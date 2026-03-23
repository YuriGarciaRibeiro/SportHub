using Application.CQRS;
using Application.UseCases.Location.CreateLocation;

namespace Application.UseCases.Tenant.ProvisionTenant;

public record ProvisionTenantCommand(
    string Slug,
    string Name,
    string? OwnerFirstName,
    string? OwnerLastName,
    string? OwnerEmail,
    string? LocationName,
    AddressRequest? Address,
    string? Phone
) : ICommand<ProvisionTenantResponse>;
