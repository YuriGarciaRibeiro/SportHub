using Domain.Entities;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

public class UserService : IUserService
{
    private readonly UserManager<AppUser> _userManager;

    public UserService(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<User> GetUserByIdAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user == null)
            throw new KeyNotFoundException("User not found.");

        var userEntity = user.ToDomain();
        return userEntity;
    }
}
