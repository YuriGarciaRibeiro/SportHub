// Application/Common/Interfaces/IBaseService.cs
using Domain.Common;

namespace Application.Common.Interfaces;

public interface IBaseService<T> where T : class, IEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<T>> GetAllAsync(CancellationToken ct = default);
    Task<T> CreateAsync(T entity, CancellationToken ct = default);
    Task UpdateAsync(T entity, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<List<T>> CreateManyAsync(IEnumerable<T> entities, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
}
