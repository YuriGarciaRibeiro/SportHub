using Application.Common.Interfaces;
using Application.CQRS;
using FluentResults;

namespace Application.UseCases.Tenant.ProvisionTenantOwner;

public class ProvisionTenantOwnerHandler : ICommandHandler<ProvisionTenantOwnerCommand>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ITenantProvisioningService _tenantProvisioningService;
    private readonly IUnitOfWork _unitOfWork;

    public ProvisionTenantOwnerHandler(
        ITenantRepository tenantRepository,
        ITenantProvisioningService tenantProvisioningService,
        IUnitOfWork unitOfWork)
    {
        _tenantRepository = tenantRepository;
        _tenantProvisioningService = tenantProvisioningService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ProvisionTenantOwnerCommand request, CancellationToken ct)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, ct);
        if (tenant is null)
        {
            return Result.Fail($"Tenant '{request.TenantId}' não encontrado.");
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return Result.Fail("O e-mail do dono/admin é obrigatório.");
        }

        // Se o Tenant global ainda não tem e-mail do dono ou é diferente, nós atualizamos a raiz.
        if (tenant.OwnerEmail != request.Email)
        {
            tenant.UpdateOwnerInfo(tenant.OwnerFirstName, tenant.OwnerLastName, request.Email);
            await _tenantRepository.UpdateAsync(tenant, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }

        await _tenantProvisioningService.ProvisionOwnerUserAsync(tenant, ct);

        return Result.Ok();
    }
}
