using Domain.Entities;

namespace Application.Common.Interfaces;

public interface ISportsRepository
{
    Task<Sport?> GetByIdAsync(Guid id);
    Task<List<Sport>> GetAllAsync();
    Task AddAsync(Sport entity);
    Task UpdateAsync(Sport entity);
    Task RemoveAsync(Sport entity);
    Task<List<Sport>> GetByIdsAsync(IEnumerable<Guid> ids);
    Task<bool> ExistsAsync(Guid id);
    IQueryable<Sport> Query();
    Task AddManyAsync(IEnumerable<Sport> entities);
    Task<Sport?> GetByNameAsync(string name);
    Task<IEnumerable<Sport>> GetSportsByIdsAsync(IEnumerable<Guid> ids);
    Task<bool> ExistsByNameAsync(string name);
}
