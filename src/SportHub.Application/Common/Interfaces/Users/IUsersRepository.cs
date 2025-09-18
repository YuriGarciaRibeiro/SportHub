using Application.Common.Interfaces.Base;
using Domain.Entities;

namespace Application.Common.Interfaces.Users;

public interface IUsersRepository : IBaseRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);

    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken);
    Task UpdatePasswordAsync(User user, string newHash, string newSalt, CancellationToken ct = default);
    Task IncrementTokenVersionAsync(User user, CancellationToken ct = default);
}
