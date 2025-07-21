using Domain.Entities;
using Domain.Enums;

public interface IUserService
{
    public Task<User> GetUserByIdAsync(Guid userId);
    public Task<Result<List<User>>> GetUsersByIdsAsync(List<Guid> userIds);
    public Task<Result<User>> AddRoleToUserAsync(Guid userId, UserRole role);
    public Task<Result<User>> RemoveRoleFromUserAsync(Guid userId, UserRole role);
}
