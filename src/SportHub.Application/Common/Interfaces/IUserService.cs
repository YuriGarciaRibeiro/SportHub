public interface IUserService
{
    public Task GetUserByIdAsync(Guid userId);
}
