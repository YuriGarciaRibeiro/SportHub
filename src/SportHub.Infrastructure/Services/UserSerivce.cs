using Domain.Entities;
using Domain.Enums;
using FluentResults;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class UserService : IUserService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;

    public UserService(UserManager<AppUser> userManager, RoleManager<IdentityRole<Guid>> roleManager)
    {
        _roleManager = roleManager;
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

    public async Task<Result<User>> AddRoleToUserAsync(Guid userId, UserRole role)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return Result.Fail("User not found.");

        var roleName = role.ToString();

        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            var create = await _roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
            if (!create.Succeeded)
            {
                var errors = string.Join(", ", create.Errors.Select(e => e.Description));
                return Result.Fail(errors);
            }
        }

        var addResult = await _userManager.AddToRoleAsync(user, roleName);
        if (!addResult.Succeeded)
        {
            var errors = string.Join(", ", addResult.Errors.Select(e => e.Description));
            return Result.Fail(errors);
        }

        return Result.Ok(user.ToDomain());
    }

    public async Task<Result<User>> RemoveRoleFromUserAsync(Guid userId, UserRole role)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return Result.Fail("User not found.");

        var roleName = role.ToString();

        if (role == UserRole.User)
            return Result.Ok(user.ToDomain());

        if (!await _roleManager.RoleExistsAsync(roleName))
            return Result.Fail($"Role '{roleName}' does not exist.");

        var remResult = await _userManager.RemoveFromRoleAsync(user, roleName);
        if (!remResult.Succeeded)
        {
            var errors = string.Join(", ", remResult.Errors.Select(e => e.Description));
            return Result.Fail(errors);
        }

        return Result.Ok(user.ToDomain());
    }
}