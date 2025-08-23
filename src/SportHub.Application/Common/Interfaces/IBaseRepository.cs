namespace Application.Common.Interfaces;

public interface IBaseRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<T>> GetAllAsync(CancellationToken cancellationToken);
    IQueryable<T> Query();
    
    Task<T?> GetByIdAsNoTrackingAsync(Guid id, CancellationToken cancellationToken);
    Task<List<T>> GetPagedAsync(int skip, int take, CancellationToken cancellationToken);
    Task<int> GetCountAsync(CancellationToken cancellationToken);
    Task<List<T>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);
    IQueryable<T> QueryAsNoTracking();
    
    Task AddAsync(T entity, CancellationToken cancellationToken);
    Task UpdateAsync(T entity, CancellationToken cancellationToken);
    Task RemoveAsync(T entity, CancellationToken cancellationToken);
    Task AddManyAsync(IEnumerable<T> entities, CancellationToken cancellationToken);
    
    Task RemoveByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken);
}
