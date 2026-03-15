using Application.Common.Interfaces;
using Application.Common.Models;
using Application.CQRS;
using FluentResults;

namespace Application.UseCases.Tenant.GetAllTenants;

public class GetAllTenantsHandler : IQueryHandler<GetAllTenantsQuery, PagedResult<GetAllTenantsResponse>>
{
    private readonly ITenantRepository _repo;

    public GetAllTenantsHandler(ITenantRepository repo) => _repo = repo;

    public async Task<Result<PagedResult<GetAllTenantsResponse>>> Handle(GetAllTenantsQuery request, CancellationToken ct)
    {
        var filter = request.Filter;

        var pagedTenants = await _repo.GetPagedAsync(
            page: filter.Page,
            pageSize: filter.PageSize,
            name: filter.Name,
            slug: filter.Slug,
            status: filter.Status,
            searchTerm: filter.SearchTerm,
            ct: ct);

        var result = new PagedResult<GetAllTenantsResponse>
        {
            Items = pagedTenants.Items.Select(t => new GetAllTenantsResponse(
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
            )).ToList(),
            TotalCount = pagedTenants.TotalCount,
            Page = pagedTenants.Page,
            PageSize = pagedTenants.PageSize
        };

        return Result.Ok(result);
    }
}
