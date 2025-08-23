using Domain.Entities;
using Domain.Enums;

namespace Application.Common.Interfaces;

public interface IEvaluationRepository : IBaseRepository<Evaluation>
{
    Task<IEnumerable<Evaluation>> GetEvaluationsByTargetAsync(Guid targetId, EvaluationTargetType targetType, CancellationToken cancellationToken);
    Task<IEnumerable<Evaluation>> GetEvaluationsByUserAsync(Guid userId, CancellationToken cancellationToken);
    Task<double> GetAverageRatingAsync(Guid targetId, EvaluationTargetType targetType, CancellationToken cancellationToken);
}
