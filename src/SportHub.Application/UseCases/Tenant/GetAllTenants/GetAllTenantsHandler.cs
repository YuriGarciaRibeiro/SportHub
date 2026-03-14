using Application.Common.Interfaces;
using Application.CQRS;
using FluentResults;

namespace Application.UseCases.Tenant.GetAllTenants;

public class GetAllTenantsHandler : IQueryHandler<GetAllTenantsQuery, List<GetAllTenantsResponse>>
{
    private readonly ITenantRepository _repo;

    public GetAllTenantsHandler(ITenantRepository repo) => _repo = repo;

    public async Task<Result<List<GetAllTenantsResponse>>> Handle(GetAllTenantsQuery request, CancellationToken ct)
    {
        var tenants = await _repo.GetAllAsync(ct);

        var response = tenants.Select(t => new GetAllTenantsResponse(
            t.Id,
            t.Slug,
            t.Name,
            t.Status.ToString(),
            t.LogoUrl,
            t.PrimaryColor,
            t.OwnerFirstName,
            t.OwnerLastName,
            t.OwnerEmail,
            t.CreatedAt
        )).ToList();

        return Result.Ok(response);
    }
}
