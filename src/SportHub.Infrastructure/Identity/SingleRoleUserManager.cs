using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class SingleRoleUserManager<TUser> : UserManager<TUser>
    where TUser : class
{
    public SingleRoleUserManager(
        IUserStore<TUser> store,
        IOptions<IdentityOptions> optionsAccessor,
        IPasswordHasher<TUser> passwordHasher,
        IEnumerable<IUserValidator<TUser>> userValidators,
        IEnumerable<IPasswordValidator<TUser>> passwordValidators,
        ILookupNormalizer keyNormalizer,
        IdentityErrorDescriber errors,
        IServiceProvider services,
        ILogger<UserManager<TUser>> logger)
        : base(store, optionsAccessor, passwordHasher, userValidators,
               passwordValidators, keyNormalizer, errors, services, logger)
    { }

    public override async Task<IdentityResult> AddToRoleAsync(
        TUser user, string role)
    {
        var current = await GetRolesAsync(user);
        if (current.Any())
        {
            var rem = await RemoveFromRolesAsync(user, current);
            if (!rem.Succeeded) return rem;
        }

        return await base.AddToRoleAsync(user, role);
    }

    public override async Task<IdentityResult> RemoveFromRoleAsync(
        TUser user, string role)
    {
        var result = await base.RemoveFromRoleAsync(user, role);
        if (!result.Succeeded) return result;

        const string defaultRole = nameof(UserRole.User);
        if (!await IsInRoleAsync(user, defaultRole))
            await base.AddToRoleAsync(user, defaultRole);

        return IdentityResult.Success;
    }
}
