using Application.Common.Models;
using Domain.Entities;

namespace Application.Common.Interfaces;

public interface ICourtsRepository
{
    Task<Court?> GetByIdAsync(Guid id);
    Task<List<Court>> GetAllAsync();
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
        string? searchTerm = null);
}
