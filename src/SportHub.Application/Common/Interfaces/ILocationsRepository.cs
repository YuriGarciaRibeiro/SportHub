using Domain.Entities;

namespace Application.Common.Interfaces;

public interface ILocationsRepository
{
    Task<List<Location>> GetAllAsync();
    Task<Location?> GetByIdAsync(Guid id);
    Task<Location?> GetDefaultAsync();
    Task AddAsync(Location entity);
    Task UpdateAsync(Location entity);
    Task RemoveAsync(Location entity);
}
