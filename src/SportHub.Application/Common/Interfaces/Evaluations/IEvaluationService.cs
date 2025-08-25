using Domain.Entities;
using Domain.Enums;

namespace Application.Common.Interfaces.Evaluations;

public interface IEvaluationService : IBaseService<Evaluation>
{
    Task<List<Evaluation>> GetEvaluationsByTargetAsync(Guid targetId, EvaluationTargetType targetType, CancellationToken ct = default);
    Task<List<Evaluation>> GetEvaluationsByUserAsync(Guid userId, CancellationToken ct = default);
    Task<double> GetAverageRatingAsync(Guid targetId, EvaluationTargetType targetType, CancellationToken ct = default);
}
