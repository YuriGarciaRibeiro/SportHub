using Domain.Entities;
using Domain.Enums;

namespace Application.Common.Interfaces;

public interface IUserService
{
    public Task<Result<User>> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken);
    public Task<Result<List<User>>> GetUsersByIdsAsync(List<Guid> userIds, CancellationToken cancellationToken);
    public Task<Result<User>> AddRoleToUserAsync(Guid userId, UserRole role, CancellationToken cancellationToken);
    public Task<Result<User>> RemoveRoleFromUserAsync(Guid userId, UserRole role, CancellationToken cancellationToken);
    public Task<Result<List<User>>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken);
}
