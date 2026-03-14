using Application.Common.Enums;
using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.CQRS;
using FluentResults;
using MediatR;

namespace Application.UseCases.Tenant.ActivateTenant;

public class ActivateTenantHandler : ICommandHandler<ActivateTenantCommand, Unit>
{
    private readonly ITenantRepository _repo;
    private readonly ICacheService _cache;
    private readonly IUnitOfWork _unitOfWork;

    public ActivateTenantHandler(ITenantRepository repo, ICacheService cache, IUnitOfWork unitOfWork)
    {
        _repo = repo;
        _cache = cache;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Unit>> Handle(ActivateTenantCommand request, CancellationToken ct)
    {
        var tenant = await _repo.GetByIdAsync(request.Id, ct);
        if (tenant is null)
            return Result.Fail(new NotFound($"Tenant '{request.Id}' não encontrado."));

        tenant.Activate();
        await _repo.UpdateAsync(tenant, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var key = _cache.GenerateCacheKey(CacheKeyPrefix.TenantBySlug, tenant.Slug);
        await _cache.RemoveAsync(key, ct);

        return Result.Ok(Unit.Value);
    }
}
