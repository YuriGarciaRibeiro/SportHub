using Domain.Entities;

namespace Application.Common.Interfaces;

public interface ITenantProvisioningService
{
    Task ProvisionAsync(Tenant tenant, CancellationToken ct = default);
    Task ProvisionOwnerUserAsync(Tenant tenant, CancellationToken ct = default);
}
