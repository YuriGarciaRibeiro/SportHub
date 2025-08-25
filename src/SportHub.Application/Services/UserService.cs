using Application.Common.Enums;
using Application.Common.Errors;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class UserService : BaseService<User>, IUserService
{
    private readonly IUsersRepository _userRepository;

    protected override TimeSpan DefaultTtl => TimeSpan.FromMinutes(30);

    public UserService(
        IUsersRepository userRepository,
        ICacheService cacheService)
        : base(userRepository, cacheService)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<User>> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await GetByIdNoTrackingAsync(userId, cancellationToken);
        return user is not null 
            ? Result.Ok(user) 
            : Result.Fail(new NotFound($"User with ID {userId} not found."));
    }

    public async Task<Result<User>> AddRoleToUserAsync(Guid userId, UserRole role, CancellationToken cancellationToken)
    {
        var user = await GetByIdAsync(userId, ct: cancellationToken);
        if (user is null)
            return Result.Fail(new NotFound($"User with ID {userId} not found."));

        user.Role = role;
        await UpdateAsync(user, cancellationToken);
        return Result.Ok(user);
    }

    public async Task<Result<User>> RemoveRoleFromUserAsync(Guid userId, UserRole role, CancellationToken cancellationToken)
    {
        var user = await GetByIdAsync(userId, ct: cancellationToken);
        if (user is null)
            return Result.Fail(new NotFound($"User with ID {userId} not found."));

        if (user.Role == role)
            user.Role = UserRole.User;

        await UpdateAsync(user, cancellationToken);
        return Result.Ok(user);
    }

    public async Task<User?> GetUserByEmailAsync(string email, CancellationToken ct = default)
    {
        return await _userRepository.GetByEmailAsync(email, ct);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
    {
        return await _userRepository.EmailExistsAsync(email, ct);
    }
}
