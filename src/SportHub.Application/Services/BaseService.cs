// Application/Services/BaseService.cs
using Application.Common.Enums;
using Application.Common.Interfaces;
using Domain.Common;

namespace Application.Services;

// Se NÃO quiser instanciar diretamente, troque para "public abstract class BaseService<T> ..."
public class BaseService<T> : IBaseService<T> where T : class, IEntity
{
    protected readonly IBaseRepository<T> _repo;
    protected readonly ICacheService _cache;

    // TTL padrão simples (30min). As filhas podem sobrescrever.
    protected virtual TimeSpan DefaultTtl => TimeSpan.FromMinutes(30);

    public BaseService(IBaseRepository<T> repo, ICacheService cache)
    {
        _repo = repo;
        _cache = cache;
    }

    // chaves de cache bem simples; as filhas podem sobrescrever se quiserem
    protected virtual string CacheKeyById(Guid id) =>
        _cache is null ? string.Empty : _cache.GenerateCacheKey(CacheKeyPrefix.EntityById, typeof(T).Name, id);

    protected virtual string CacheKeyAll() =>
        _cache is null ? string.Empty : _cache.GenerateCacheKey(CacheKeyPrefix.EntityAll, typeof(T).Name);

    public virtual async Task<T?> GetByIdAsync(Guid id, TimeSpan? ttl = null, CancellationToken ct = default)
    {
        
        var key = CacheKeyById(id);
        var cached = await _cache.GetAsync<T>(key, ct);
        if (cached is not null) return cached;
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is not null) await _cache.SetAsync(key, entity, ttl ?? DefaultTtl, ct);
        return entity;
    }

    public virtual async Task<List<T>> GetAllAsync(TimeSpan? ttl = null, CancellationToken ct = default)
    {
        
        var key = CacheKeyAll();
        var cached = await _cache.GetAsync<List<T>>(key, ct);
        if (cached is not null) return cached;
        var list = await _repo.GetAllAsync(ct);
        await _cache.SetAsync(key, list, ttl ?? DefaultTtl, ct);
        return list;
        
    }

    public virtual async Task<T> CreateAsync(T entity, CancellationToken ct = default)
    {
        await _repo.AddAsync(entity, ct);
        await _cache.RemoveAsync(CacheKeyAll(), ct);
        await _cache.SetAsync(CacheKeyById(entity.Id), entity, DefaultTtl, ct);
        return entity;
    }

    public virtual async Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        await _repo.UpdateAsync(entity, ct);
        await _cache.RemoveAsync(CacheKeyAll(), ct);
        await _cache.SetAsync(CacheKeyById(entity.Id), entity, DefaultTtl, ct);

    }

    public virtual async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null) return;

        await _repo.RemoveAsync(entity, ct);

        await _cache.RemoveAsync(CacheKeyById(id), ct);
        await _cache.RemoveAsync(CacheKeyAll(), ct);

    }

    public virtual async Task<List<T>> CreateManyAsync(IEnumerable<T> entities, CancellationToken ct = default)
    {
        var list = entities as IList<T> ?? entities.ToList();
        await _repo.AddManyAsync(list, ct);

        foreach (var e in list)
            await _cache.SetAsync(CacheKeyById(e.Id), e, DefaultTtl, ct);
        await _cache.RemoveAsync(CacheKeyAll(), ct);

        return list.ToList();
    }

    public Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
        => _repo.ExistsAsync(id, ct);
}
