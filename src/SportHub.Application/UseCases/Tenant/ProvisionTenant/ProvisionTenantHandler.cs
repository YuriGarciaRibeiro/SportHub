using Application.Common.Interfaces;
using Application.CQRS;
using FluentResults;
using TenantEntity = Domain.Entities.Tenant;

namespace Application.UseCases.Tenant.ProvisionTenant;

public class ProvisionTenantHandler : ICommandHandler<ProvisionTenantCommand, ProvisionTenantResponse>
{
    private readonly ITenantProvisioningService _provisioning;

    public ProvisionTenantHandler(ITenantProvisioningService provisioning)
    {
        _provisioning = provisioning;
    }

    public async Task<Result<ProvisionTenantResponse>> Handle(
        ProvisionTenantCommand request, CancellationToken ct)
    {
        var tenant = TenantEntity.Create(request.Slug, request.Name, request.OwnerFirstName, request.OwnerLastName, request.OwnerEmail);

        await _provisioning.ProvisionAsync(tenant, ct);

        return Result.Ok(new ProvisionTenantResponse(
            tenant.Id,
            tenant.Slug,
            tenant.Name,
            tenant.GetSchemaName()
        ));
    }
}
