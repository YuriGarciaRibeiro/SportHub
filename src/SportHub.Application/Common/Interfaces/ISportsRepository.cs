using Domain.Entities;

namespace Application.Common.Interfaces;

public interface ISportsRepository : IBaseRepository<Sport>
{
    Task<Sport?> GetByNameAsync(string name);
    Task<IEnumerable<Sport>> GetSportsByIdsAsync(IEnumerable<Guid> ids);
    Task<bool> ExistsByNameAsync(string name);
}
