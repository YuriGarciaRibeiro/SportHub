using Domain.Entities;
using Domain.Entities;

namespace Application.Common.Interfaces;

public interface ITenantUsersQueryService
{
    Task<List<UserDto>> GetAdminUsersAsync(Tenant tenant, CancellationToken ct = default);
}

public record UserDto(Guid Id, string FirstName, string LastName, string Email, string Role);
