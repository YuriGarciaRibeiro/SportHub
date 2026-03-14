using Application.Common.Interfaces;
using Application.CQRS;
using FluentResults;

namespace Application.UseCases.Tenant.GetTenantUsers;

public class GetTenantUsersHandler : IQueryHandler<GetTenantUsersQuery, List<GetTenantUsersResponse>>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ITenantUsersQueryService _queryService;

    public GetTenantUsersHandler(ITenantRepository tenantRepository, ITenantUsersQueryService queryService)
    {
        _tenantRepository = tenantRepository;
        _queryService = queryService;
    }

    public async Task<Result<List<GetTenantUsersResponse>>> Handle(GetTenantUsersQuery request, CancellationToken ct)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, ct);
        if (tenant is null)
        {
            return Result.Fail($"Tenant '{request.TenantId}' não encontrado.");
        }

        var users = await _queryService.GetAdminUsersAsync(tenant, ct);

        var response = users.Select(u => new GetTenantUsersResponse(
            u.Id,
            u.FirstName,
            u.LastName,
            u.Email,
            u.Role
        )).ToList();

        return Result.Ok(response);
    }
}
