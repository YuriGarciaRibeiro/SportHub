// Application/Services/BaseService.cs
using Application.Common.Enums;
using Domain.Common;

namespace Application.Services;


public class BaseService<T> : IBaseService<T> where T : class, IEntity
{
    protected readonly IBaseRepository<T> _repo;
    protected readonly ICacheService _cache;

    protected virtual TimeSpan DefaultTtl => TimeSpan.FromMinutes(30);
    protected virtual TimeSpan ShortTtl => TimeSpan.FromMinutes(5);

    public BaseService(IBaseRepository<T> repo, ICacheService cache)
    {
        _repo = repo;
        _cache = cache;
    }

    protected virtual string CacheKeyById(Guid id) =>
        _cache is null ? string.Empty : _cache.GenerateCacheKey(CacheKeyPrefix.EntityById, typeof(T).Name, id);

    protected virtual string CacheKeyAll() =>
        _cache is null ? string.Empty : _cache.GenerateCacheKey(CacheKeyPrefix.EntityAll, typeof(T).Name);

    protected virtual string CacheKeyPaged(int skip, int take) =>
        _cache is null ? string.Empty : _cache.GenerateCacheKey(CacheKeyPrefix.EntityPaged, typeof(T).Name, $"{skip}_{take}");

    protected virtual string CacheKeyCount() =>
        _cache is null ? string.Empty : _cache.GenerateCacheKey(CacheKeyPrefix.EntityCount, typeof(T).Name);

    protected virtual string CacheKeyByIdComplete(Guid id) =>
        _cache is null ? string.Empty : _cache.GenerateCacheKey(CacheKeyPrefix.EntityByIdComplete, typeof(T).Name, id);

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _repo.GetByIdAsync(id, ct);
    }

    public virtual async Task<T?> GetByIdNoTrackingAsync(Guid id, CancellationToken ct = default)
    {
        var key = CacheKeyById(id);
        var cached = await _cache.GetAsync<T>(key, ct);
        if (cached is not null) return cached;
        var entity = await _repo.GetByIdAsNoTrackingAsync(id, ct);
        if (entity is not null) await _cache.SetAsync(key, entity, DefaultTtl, ct);
        return entity;
    }

    public virtual async Task<List<T>> GetAllAsync( CancellationToken ct = default)
    {
        return await _repo.GetAllAsync(ct);
    }

    public virtual async Task<List<T>> GetPagedAsync(int skip, int take, TimeSpan? ttl = null, CancellationToken ct = default)
    {
        var key = CacheKeyPaged(skip, take);
        var cached = await _cache.GetAsync<List<T>>(key, ct);
        if (cached is not null) return cached;
        
        var list = await _repo.GetPagedAsync(skip, take, ct);
        await _cache.SetAsync(key, list, ttl ?? DefaultTtl, ct);
        return list;
    }

    public virtual async Task<int> GetCountAsync(CancellationToken ct = default)
    {
        var key = CacheKeyCount();
        var cached = await _cache.GetAsync<string>(key, ct);
        if (cached != null && int.TryParse(cached, out var cachedCount)) 
            return cachedCount;
        
        var count = await _repo.GetCountAsync(ct);
        await _cache.SetAsync(key, count.ToString(), DefaultTtl, ct);
        return count;
    }

    public virtual async Task<List<T>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
    {
        var idList = ids.ToList();
        var idsHash = string.Join(",", idList.OrderBy(x => x)).GetHashCode();
        var key = _cache.GenerateCacheKey("EntityByIds", typeof(T).Name, idsHash.ToString());
        
        var cached = await _cache.GetAsync<List<T>>(key, ct);
        if (cached is not null) return cached;
        
        var result = await _repo.GetByIdsAsync(idList, ct);
        await _cache.SetAsync(key, result, ShortTtl, ct);
        return result;
    }

    public virtual async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        var entityKey = CacheKeyById(id);
        var cached = await _cache.GetAsync<T>(entityKey, ct);
        if (cached is not null) return true;
        
        var existsKey = _cache.GenerateCacheKey("EntityExists", typeof(T).Name, id.ToString());
        var existsCached = await _cache.GetAsync<string>(existsKey, ct);
        if (existsCached != null) return bool.Parse(existsCached);
        
        var exists = await _repo.ExistsAsync(id, ct);
        await _cache.SetAsync(existsKey, exists.ToString(), DefaultTtl, ct);
        return exists;
    }

    public virtual async Task<T> CreateAsync(T entity, CancellationToken ct = default)
    {
        await _repo.AddAsync(entity, ct);
        
        await InvalidateCacheAsync(ct: ct);
        
        await _cache.SetAsync(CacheKeyById(entity.Id), entity, DefaultTtl, ct);
        return entity;
    }

    public virtual async Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        await _repo.UpdateAsync(entity, ct);
        
        await InvalidateCacheAsync(entity.Id, ct);
        
        await _cache.SetAsync(CacheKeyById(entity.Id), entity, DefaultTtl, ct);
    }

    public virtual async Task DeleteAsync(T entity, CancellationToken ct = default)
    {
        if (entity is null) return;

        await _repo.RemoveAsync(entity, ct);

        await InvalidateCacheAsync(entity.Id, ct);
    }

    public virtual async Task DeleteManyAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
    {
        var idList = ids.ToList();
        if (!idList.Any()) return;

        await _repo.RemoveByIdsAsync(idList, ct);

        await InvalidateCacheAsync(ct: ct);
        
        foreach (var id in idList)
        {
            await _cache.RemoveAsync(CacheKeyById(id), ct);
        }
    }

    public virtual async Task<List<T>> CreateManyAsync(IEnumerable<T> entities, CancellationToken ct = default)
    {
        var list = entities as IList<T> ?? entities.ToList();
        await _repo.AddManyAsync(list, ct);

        await InvalidateCacheAsync(ct: ct);

        foreach (var e in list)
            await _cache.SetAsync(CacheKeyById(e.Id), e, DefaultTtl, ct);

        return list.ToList();
    }

    public virtual async Task InvalidateCacheAsync(Guid? id = null, CancellationToken ct = default)
    {
        if (id.HasValue)
        {
            await _cache.RemoveAsync(CacheKeyById(id.Value), ct);
            await _cache.RemoveAsync(CacheKeyByIdComplete(id.Value), ct);
        }

        await _cache.RemoveAsync(CacheKeyAll(), ct);

        await _cache.RemoveAsync(CacheKeyCount(), ct);
        
        var commonPageSizes = new[] { 10, 20, 50, 100 };

        for (int i = 0; i < 5; i++)
        {
            foreach (var pageSize in commonPageSizes)
            {
                await _cache.RemoveAsync(CacheKeyPaged(i * pageSize, pageSize), ct);
            }
        }
    }
}
