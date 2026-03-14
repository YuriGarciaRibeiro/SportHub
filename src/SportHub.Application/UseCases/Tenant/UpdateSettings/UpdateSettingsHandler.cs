using Application.Common.Enums;
using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.CQRS;
using FluentResults;
using MediatR;

namespace Application.UseCases.Tenant.UpdateSettings;

public class UpdateSettingsHandler : ICommandHandler<UpdateSettingsCommand, Unit>
{
    private readonly ITenantRepository _repo;
    private readonly ITenantContext _tenantContext;
    private readonly ICacheService _cache;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateSettingsHandler(ITenantRepository repo, ITenantContext tenantContext, ICacheService cache, IUnitOfWork unitOfWork)
    {
        _repo = repo;
        _tenantContext = tenantContext;
        _cache = cache;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Unit>> Handle(UpdateSettingsCommand request, CancellationToken ct)
    {
        if (!_tenantContext.IsResolved)
            return Result.Fail(new Unauthorized("Tenant não resolvido no contexto."));

        var tenant = await _repo.GetByIdAsync(_tenantContext.TenantId, ct);
        if (tenant is null)
            return Result.Fail(new NotFound("Tenant não encontrado."));

        tenant.UpdateSettings(request.Name, request.LogoUrl, request.PrimaryColor);
        await _repo.UpdateAsync(tenant, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var key = _cache.GenerateCacheKey(CacheKeyPrefix.TenantBySlug, tenant.Slug);
        await _cache.RemoveAsync(key, ct);

        // Update context if we need to return it, but here just Unit
        return Result.Ok(Unit.Value);
    }
}
