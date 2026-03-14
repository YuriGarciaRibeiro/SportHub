using Application.CQRS;

namespace Application.UseCases.Tenant.GetTenant;

public record GetTenantQuery(Guid Id) : IQuery<GetTenantResponse>;
