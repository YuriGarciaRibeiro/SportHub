using Application.Common.Models;
using Application.CQRS;

namespace Application.UseCases.Tenant.GetTenantUsers;

public record GetTenantUsersQuery(Guid TenantId, GetTenantUsersFilter Filter) : IQuery<PagedResult<GetTenantUsersResponse>>;

public class GetTenantUsersFilter
{
    public int? Page { get; set; }
    public int? PageSize { get; set; }
    public string? SearchTerm { get; set; }
    public string? Role { get; set; }
}
