---
name: sporthub-cache
description: >
  Guides adding Redis caching to SportHub handlers following the project's ICacheService pattern — including cache-aside for queries and cache invalidation for commands.
  Use this skill whenever the user wants to add caching, improve performance, cache a query result, invalidate cache after a mutation, add a new CacheKeyPrefix, or asks why cached data is stale. Trigger any time Redis cache or ICacheService is involved in SportHub, even if the user just says "cachear X" or "invalidar cache de Y".
---

# SportHub Cache

Adds Redis caching to SportHub handlers using the project's `ICacheService` — cache-aside pattern for queries, invalidation for commands.

## Project context

- **Interface:** `Application.Common.Interfaces.ICacheService`
- **Key prefixes enum:** `Application.Common.Enums.CacheKeyPrefix` — add new entries here for each new cached resource
- **Default TTL:** 30 minutes (`TimeSpan.FromMinutes(30)`)
- **Implementation:** Redis via `IDistributedCache` (wired in DI automatically)

## ICacheService API

```csharp
// Read from cache
T? value = await _cache.GetAsync<T>(key, cancellationToken);

// Write to cache
await _cache.SetAsync(key, value, TimeSpan.FromMinutes(30), cancellationToken);

// Invalidate
await _cache.RemoveAsync(key, cancellationToken);

// Check existence
bool exists = await _cache.ExistsAsync(key, cancellationToken);

// Build a consistent key
string key = _cache.GenerateCacheKey(CacheKeyPrefix.SomePrefix, arg1, arg2, ...);
```

## Caching patterns

### Cache-aside in a Query handler

```csharp
public async Task<Result<MyResponse>> Handle(MyQuery request, CancellationToken ct)
{
    var key = _cache.GenerateCacheKey(CacheKeyPrefix.MyPrefix, request.SomeId);
    
    var cached = await _cache.GetAsync<MyResponse>(key, ct);
    if (cached != null)
        return Result.Ok(cached);

    // fetch from DB
    var data = await _repository.GetAsync(...);
    if (data is null)
        return Result.Fail(new NotFound("..."));

    var response = MapToResponse(data);
    await _cache.SetAsync(key, response, TimeSpan.FromMinutes(30), ct);
    return Result.Ok(response);
}
```

### Cache invalidation in a Command handler

```csharp
public async Task<Result> Handle(UpdateMyEntityCommand request, CancellationToken ct)
{
    var entity = await _repository.GetByIdAsync(request.Id);
    if (entity is null)
        return Result.Fail(new NotFound("..."));

    // ... apply changes, save ...
    await _unitOfWork.SaveChangesAsync(ct);

    // invalidate related cache entries
    var key = _cache.GenerateCacheKey(CacheKeyPrefix.MyPrefix, entity.SomeId);
    await _cache.RemoveAsync(key, ct);

    return Result.Ok();
}
```

## Step-by-step guide

### 1. Add a CacheKeyPrefix entry

Open `src/SportHub.Application/Common/Enums/CacheKeyPrefix.cs` and add the new entry:

```csharp
public enum CacheKeyPrefix
{
    UserById,
    CourtAvailability,
    ReservationList,
    EstablishmentSummary,
    GetAvailability,
    TenantBySlug,
    TenantBranding,
    {NewPrefix},   // ← add here
}
```

Name it after the resource being cached, not the handler (e.g. `TournamentList`, `MemberById`).

### 2. Add ICacheService to the Query handler

Inject `ICacheService` alongside the repository:

```csharp
public class MyQueryHandler : IQueryHandler<MyQuery, MyResponse>
{
    private readonly IMyRepository _repository;
    private readonly ICacheService _cache;

    public MyQueryHandler(IMyRepository repository, ICacheService cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<Result<MyResponse>> Handle(MyQuery request, CancellationToken ct)
    {
        var key = _cache.GenerateCacheKey(CacheKeyPrefix.MyPrefix, /* discriminating args */);

        var cached = await _cache.GetAsync<MyResponse>(key, ct);
        if (cached != null)
            return Result.Ok(cached);

        var data = await _repository.GetAsync(...);
        if (data is null)
            return Result.Fail(new NotFound("..."));

        var response = /* map to response */;
        await _cache.SetAsync(key, response, TimeSpan.FromMinutes(30), ct);
        return Result.Ok(response);
    }
}
```

### 3. Invalidate in the related Command handlers

Every command that mutates the cached resource must remove the cache entry after saving:

```csharp
// after _unitOfWork.SaveChangesAsync(ct)
var key = _cache.GenerateCacheKey(CacheKeyPrefix.MyPrefix, entity.RelevantId);
await _cache.RemoveAsync(key, ct);
```

If a command can affect multiple cached entries (e.g. updating a court affects both `CourtAvailability` and a `CourtById` key), remove all of them.

## Cache key design rules

- **Include all discriminating values in the key.** A query filtered by `(courtId, date)` needs both in the key: `GenerateCacheKey(CacheKeyPrefix.GetAvailability, courtId, date.ToString("yyyy-MM-dd"))`.
- **Tenant isolation is implicit** — keys already include tenant-scoped IDs (like `courtId`) that make them unique per tenant. You don't need to manually include `TenantId` if the resource ID already does it.
- **Platform-level resources** (e.g. `TenantBySlug`) use the slug as the discriminator.

## TTL guidelines

| Data type | Suggested TTL |
|---|---|
| Availability / schedule | 30 min |
| Entity details (rarely changed) | 30 min |
| Tenant branding / settings | 1 hour |
| Short-lived computed results | 5–15 min |

Use `TimeSpan.FromMinutes(N)` — never hardcode seconds.

## Common mistakes

| Mistake | Fix |
|---|---|
| Caching in a Command handler instead of Query | Only cache in Query handlers; Commands invalidate |
| Missing invalidation after update/delete | Every mutating Command must call `RemoveAsync` for affected keys |
| Using a hardcoded string key | Always use `GenerateCacheKey` — it guarantees consistent formatting |
| Caching the domain entity directly | Cache the Response DTO, not the entity — entities have EF navigation properties that don't serialize cleanly |
| Using the same key prefix for different shapes | One prefix per response type; if the shape differs, use a different prefix |
