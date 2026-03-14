using Application.CQRS;

namespace Application.UseCases.Tenant.ProvisionTenant;

public record ProvisionTenantCommand(string Slug, string Name, string? OwnerFirstName, string? OwnerLastName, string? OwnerEmail) : ICommand<ProvisionTenantResponse>;
