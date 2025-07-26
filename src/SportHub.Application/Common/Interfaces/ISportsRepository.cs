using Domain.Entities;

namespace Application.Common.Interfaces;

public interface ISportsRepository
{
    Task<Sport?> GetByIdAsync(Guid id);
    Task<Sport?> GetByNameAsync(string name);
    Task<IEnumerable<Sport>> GetAllAsync();
    Task CreateAsync(Sport sport);
    Task UpdateAsync(Sport sport);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(string name);
    Task<IEnumerable<Sport>> GetSportsByIdsAsync(IEnumerable<Guid> ids);
}
