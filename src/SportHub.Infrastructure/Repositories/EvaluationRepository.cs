using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class EvaluationRepository : BaseRepository<Evaluation>, IEvaluationRepository
{
    private readonly ApplicationDbContext _dbContext;

    public EvaluationRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<double> GetAverageRatingAsync(Guid targetId, EvaluationTargetType targetType, CancellationToken cancellationToken)
    {
        return await _dbContext.Evaluations
            .Where(e => e.TargetId == targetId && e.TargetType == targetType)
            .AverageAsync(e => e.Rating, cancellationToken);
    }

    public async Task<IEnumerable<Evaluation>> GetEvaluationsByTargetAsync(Guid targetId, EvaluationTargetType targetType, CancellationToken cancellationToken)
    {
        return await _dbContext.Evaluations
            .Where(e => e.TargetId == targetId && e.TargetType == targetType)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Evaluation>> GetEvaluationsByUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _dbContext.Evaluations
            .Where(e => e.UserId == userId)
            .ToListAsync(cancellationToken);
    }
}
