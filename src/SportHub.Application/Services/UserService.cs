using Application.Common.Enums;
using Application.Common.Errors;
using Application.Common.Interfaces;
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
        var user = await GetByIdAsync(userId, ct: cancellationToken);
        return user is not null 
            ? Result.Ok(user) 
            : Result.Fail(new NotFound($"User with ID {userId} not found."));
    }

    public async Task<Result<List<User>>> GetUsersByIdsAsync(List<Guid> userIds, CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetByIdsAsync(userIds, cancellationToken);
        return Result.Ok(users.ToList());
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

        // Remove role logic - set to default User role if removing current role
        if (user.Role == role)
            user.Role = UserRole.User;

        await UpdateAsync(user, cancellationToken);
        return Result.Ok(user);
    }

    public async Task<Result<List<User>>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetByIdsAsync(ids, cancellationToken);
        return Result.Ok(users.ToList());
    }

    public async Task<User?> GetUserByEmailAsync(string email, CancellationToken ct = default)
    {
        var key = _cache.GenerateCacheKey(CacheKeyPrefix.Query, nameof(User), "byEmail", email);
        var cached = await _cache.GetAsync<User>(key, ct);
        if (cached is not null) return cached;

        var user = await _userRepository.GetByEmailAsync(email, ct);
        if (user is not null)
            await _cache.SetAsync(key, user, DefaultTtl, ct);

        return user;
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
    {
        var key = _cache.GenerateCacheKey(CacheKeyPrefix.Query, nameof(User), "emailExists", email);
        var cached = await _cache.GetAsync<string>(key, ct);
        if (cached is not null && bool.TryParse(cached, out var cachedResult)) 
            return cachedResult;

        var exists = await _userRepository.EmailExistsAsync(email, ct);
        await _cache.SetAsync(key, exists.ToString(), TimeSpan.FromMinutes(10), ct); // Cache por menos tempo
        
        return exists;
    }
}
