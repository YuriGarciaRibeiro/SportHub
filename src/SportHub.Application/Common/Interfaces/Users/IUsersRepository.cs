using Domain.Entities;

namespace Application.Common.Interfaces.Users;

public interface IUsersRepository : IBaseRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken);
}
