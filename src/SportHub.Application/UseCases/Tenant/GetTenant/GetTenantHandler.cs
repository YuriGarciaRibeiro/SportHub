using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.CQRS;
using FluentResults;

namespace Application.UseCases.Tenant.GetTenant;

public class GetTenantHandler : IQueryHandler<GetTenantQuery, GetTenantResponse>
{
    private readonly ITenantRepository _repo;

    public GetTenantHandler(ITenantRepository repo) => _repo = repo;

    public async Task<Result<GetTenantResponse>> Handle(GetTenantQuery request, CancellationToken ct)
    {
        var tenant = await _repo.GetByIdAsync(request.Id, ct);
        if (tenant is null)
            return Result.Fail(new NotFound($"Tenant '{request.Id}' não encontrado."));

        return Result.Ok(new GetTenantResponse(
            tenant.Id,
            tenant.Slug,
            tenant.Name,
            tenant.Status.ToString(),
            tenant.LogoUrl,
            tenant.PrimaryColor,
            tenant.CustomDomain,
            tenant.OwnerFirstName,
            tenant.OwnerLastName,
            tenant.OwnerEmail,
            tenant.CreatedAt
        ));
    }
}
