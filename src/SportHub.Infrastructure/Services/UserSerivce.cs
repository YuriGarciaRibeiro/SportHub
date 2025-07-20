using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

public class UserService : IUserService
{
    private readonly UserManager<AppUser> _userManager;

    public UserService(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<AppUser> GetUserByIdAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user;
    }

    Task IUserService.GetUserByIdAsync(Guid userId)
    {
        return GetUserByIdAsync(userId);
    }
}
