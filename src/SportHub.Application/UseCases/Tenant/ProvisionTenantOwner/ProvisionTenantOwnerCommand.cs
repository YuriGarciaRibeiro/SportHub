using Application.CQRS;

namespace Application.UseCases.Tenant.ProvisionTenantOwner;

public record ProvisionTenantOwnerCommand(Guid TenantId, string Email) : ICommand;
