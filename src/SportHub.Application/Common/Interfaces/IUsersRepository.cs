using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IUsersRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
    Task DeleteAsync(Guid id);
    Task<bool> EmailExistsAsync(string email);
    Task<List<User>> GetByIdsAsync(IEnumerable<Guid> ids);
}
