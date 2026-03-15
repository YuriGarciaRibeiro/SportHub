using Application.Common.Models;
using Application.CQRS;
using Domain.Enums;

namespace Application.UseCases.Tenant.GetAllTenants;

public record GetAllTenantsQuery(GetTenantsFilter Filter) : IQuery<PagedResult<GetAllTenantsResponse>>;

public class GetTenantsFilter
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Name { get; set; }
    public string? Slug { get; set; }
    public TenantStatus? Status { get; set; }
    public string? SearchTerm { get; set; }
}
