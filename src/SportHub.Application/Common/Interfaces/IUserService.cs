using Domain.Entities;

public interface IUserService
{
    public Task<User> GetUserByIdAsync(Guid userId);
}
