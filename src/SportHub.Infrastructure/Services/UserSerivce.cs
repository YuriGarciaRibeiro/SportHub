using Domain.Entities;
using FluentResults;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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
    
    public async Task<Result<List<User>>> GetUsersByIdsAsync(List<Guid> userIds)
    {
        var users = await _userManager.Users
            .Where(u => userIds.Contains(u.Id))
            .ToListAsync();

        var mappedUsers = users.Select(MapToUser).ToList();

        return Result.Ok(mappedUsers);
    }

    private static User MapToUser(AppUser appUser)
    {
        return new User
        {
            Id = appUser.Id,
            FirstName = appUser.FirstName,
            LastName = appUser.LastName,
            Email = appUser.Email!
        };
    }
}
