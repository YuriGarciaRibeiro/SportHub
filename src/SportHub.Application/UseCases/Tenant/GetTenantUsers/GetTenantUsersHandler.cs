using Application.Common.Interfaces;
using Application.Common.Models;
using Application.CQRS;
using FluentResults;

namespace Application.UseCases.Tenant.GetTenantUsers;

public class GetTenantUsersHandler : IQueryHandler<GetTenantUsersQuery, PagedResult<GetTenantUsersResponse>>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ITenantUsersQueryService _queryService;

    public GetTenantUsersHandler(ITenantRepository tenantRepository, ITenantUsersQueryService queryService)
    {
        _tenantRepository = tenantRepository;
        _queryService = queryService;
    }

    public async Task<Result<PagedResult<GetTenantUsersResponse>>> Handle(GetTenantUsersQuery request, CancellationToken ct)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, ct);
        if (tenant is null)
            return Result.Fail($"Tenant '{request.TenantId}' não encontrado.");

        var filter = request.Filter;

        var paged = await _queryService.GetPagedUsersAsync(tenant, filter.Page ?? 1, filter.PageSize ?? 10, filter.SearchTerm, filter.Role, ct);

        var result = new PagedResult<GetTenantUsersResponse>
        {
            Items = [..paged.Items.Select(u => new GetTenantUsersResponse(u.Id, u.FirstName, u.LastName, u.Email, u.Role))],
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };

        return Result.Ok(result);
    }
}
