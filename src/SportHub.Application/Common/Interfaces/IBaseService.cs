// Application/Common/Interfaces/IBaseService.cs
using Domain.Common;

namespace Application.Common.Interfaces;

public interface IBaseService<T> where T : class, IEntity
{

    Task<T?> GetByIdAsync(Guid id, TimeSpan? ttl = null, CancellationToken ct = default);
    Task<List<T>> GetAllAsync(TimeSpan? ttl = null, CancellationToken ct = default);
    
    Task<T?> GetByIdNoTrackingAsync(Guid id, CancellationToken ct = default);
    Task<List<T>> GetPagedAsync(int skip, int take, TimeSpan? ttl = null, CancellationToken ct = default);
    Task<int> GetCountAsync(CancellationToken ct = default);
    Task<List<T>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
    
    Task<T> CreateAsync(T entity, CancellationToken ct = default);
    Task UpdateAsync(T entity, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<List<T>> CreateManyAsync(IEnumerable<T> entities, CancellationToken ct = default);
    Task DeleteManyAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
    
    // Utilitários de cache
    Task InvalidateCacheAsync(Guid? id = null, CancellationToken ct = default);
}
