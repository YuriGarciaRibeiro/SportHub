using Application.Common.Enums;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class EvaluationService : BaseService<Evaluation>, IEvaluationService
{
    private readonly IEvaluationRepository _evaluationRepository;

    protected override TimeSpan DefaultTtl => TimeSpan.FromMinutes(15); // Evaluations change more frequently

    public EvaluationService(
        IEvaluationRepository evaluationRepository,
        ICacheService cacheService)
        : base(evaluationRepository, cacheService)
    {
        _evaluationRepository = evaluationRepository;
    }

    public async Task<List<Evaluation>> GetEvaluationsByTargetAsync(Guid targetId, EvaluationTargetType targetType, CancellationToken ct = default)
    {
        var key = _cache.GenerateCacheKey(CacheKeyPrefix.Query, nameof(Evaluation), "byTarget", targetId, targetType);
        var cached = await _cache.GetAsync<List<Evaluation>>(key, ct);
        if (cached is not null) return cached;

        var evaluations = await _evaluationRepository.GetEvaluationsByTargetAsync(targetId, targetType, ct);
        var evaluationsList = evaluations.ToList();
        await _cache.SetAsync(key, evaluationsList, DefaultTtl, ct);
        return evaluationsList;
    }

    public async Task<List<Evaluation>> GetEvaluationsByUserAsync(Guid userId, CancellationToken ct = default)
    {
        var key = _cache.GenerateCacheKey(CacheKeyPrefix.Query, nameof(Evaluation), "byUser", userId);
        var cached = await _cache.GetAsync<List<Evaluation>>(key, ct);
        if (cached is not null) return cached;

        var evaluations = await _evaluationRepository.GetEvaluationsByUserAsync(userId, ct);
        var evaluationsList = evaluations.ToList();
        await _cache.SetAsync(key, evaluationsList, DefaultTtl, ct);
        return evaluationsList;
    }

    public async Task<double> GetAverageRatingAsync(Guid targetId, EvaluationTargetType targetType, CancellationToken ct = default)
    {
        var key = _cache.GenerateCacheKey(CacheKeyPrefix.Query, nameof(Evaluation), "avgRating", targetId, targetType);
        var cached = await _cache.GetAsync<string>(key, ct);
        if (cached is not null && double.TryParse(cached, out var cachedValue)) 
            return cachedValue;

        var average = await _evaluationRepository.GetAverageRatingAsync(targetId, targetType, ct);
        await _cache.SetAsync(key, average.ToString(), DefaultTtl, ct);
        return average;
    }
}
