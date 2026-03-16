using Application.Common.Models;
using Domain.Entities;

namespace Application.Common.Interfaces;

public interface ITenantUsersQueryService
{
    Task<List<UserDto>> GetAdminUsersAsync(Tenant tenant, CancellationToken ct = default);
    Task<PagedResult<UserDto>> GetPagedUsersAsync(Tenant tenant, int page, int pageSize, string? searchTerm = null, string? role = null, CancellationToken ct = default);
}

public record UserDto(Guid Id, string FirstName, string LastName, string Email, string Role);
