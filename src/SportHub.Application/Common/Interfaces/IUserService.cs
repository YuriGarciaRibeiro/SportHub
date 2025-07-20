using Domain.Entities;

public interface IUserService
{
    public Task<User> GetUserByIdAsync(Guid userId);
    public Task<Result<List<User>>> GetUsersByIdsAsync(List<Guid> userIds);
}
