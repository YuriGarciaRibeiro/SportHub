using Application.Common.Models;
using Domain.Entities;

namespace Application.Common.Interfaces;

public interface ICourtsRepository
{
    Task<Court?> GetByIdAsync(Guid id, GetCourtIncludeSettings? includeSettings = null);
    Task<List<Court>> GetAllAsync(GetCourtIncludeSettings? includeSettings = null);
    Task AddAsync(Court entity);
    Task UpdateAsync(Court entity);
    Task RemoveAsync(Court entity);
    Task<List<Court>> GetByIdsAsync(IEnumerable<Guid> ids);
    Task<bool> ExistsAsync(Guid id);
    IQueryable<Court> Query();
    Task AddManyAsync(IEnumerable<Court> entities);
    Task<PagedResult<Court>> GetPagedAsync(
        int page,
        int pageSize,
        string? name = null,
        Guid? sportId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        string? searchTerm = null,
        Guid? locationId = null);
    Task<List<Court>> GetByTenantIdsAsync(IEnumerable<Guid> tenantIds);
    Task UpdateManyAsync(IEnumerable<Court> entities);
}

public class GetCourtIncludeSettings
{
    public bool IncludeSports { get; set; } = true;
    public bool IncludeLocation { get; set; } = true;
    public bool IncludeTenant { get; set; } = false;
    public bool IncludeMaintenances { get; set; } = false;
    public bool AsNoTracking { get; set; } = true;
}
