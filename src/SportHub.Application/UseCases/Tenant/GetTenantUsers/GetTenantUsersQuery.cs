using Application.CQRS;

namespace Application.UseCases.Tenant.GetTenantUsers;

public record GetTenantUsersQuery(Guid TenantId) : IQuery<List<GetTenantUsersResponse>>;
