using Application.CQRS;

namespace Application.UseCases.Tenant.GetAllTenants;

public record GetAllTenantsQuery : IQuery<List<GetAllTenantsResponse>>;
