using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IUsersRepository : IBaseRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<bool> EmailExistsAsync(string email);
}
