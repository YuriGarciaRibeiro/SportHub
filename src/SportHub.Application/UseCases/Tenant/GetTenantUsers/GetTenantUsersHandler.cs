using Application.Common.Interfaces;
using Application.Common.Models;
using Application.CQRS;
using Domain.Enums;
using FluentResults;

namespace Application.UseCases.Tenant.GetTenantUsers;

public class GetTenantUsersHandler : IQueryHandler<GetTenantUsersQuery, PagedResult<GetTenantUsersResponse>>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUsersRepository _usersRepository;

    public GetTenantUsersHandler(ITenantRepository tenantRepository, IUsersRepository usersRepository)
    {
        _tenantRepository = tenantRepository;
        _usersRepository = usersRepository;
    }

    public async Task<Result<PagedResult<GetTenantUsersResponse>>> Handle(GetTenantUsersQuery request, CancellationToken ct)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, ct);
        if (tenant is null)
            return Result.Fail($"Tenant '{request.TenantId}' não encontrado.");

        var filter = request.Filter;

        UserRole? role = null;
        if (!string.IsNullOrWhiteSpace(filter.Role) && Enum.TryParse<UserRole>(filter.Role, ignoreCase: true, out var parsedRole))
            role = parsedRole;

        var paged = await _usersRepository.GetPagedByTenantAsync(
            tenant.Id,
            filter.Page ?? 1,
            filter.PageSize ?? 10,
            filter.SearchTerm,
            role,
            ct);

        return Result.Ok(new PagedResult<GetTenantUsersResponse>
        {
            Items = [..paged.Items.Select(u => new GetTenantUsersResponse(u.Id, u.FirstName, u.LastName, u.Email, u.Role.ToString()))],
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        });
    }
}
