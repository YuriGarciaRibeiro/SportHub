using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class Evaluation : AuditEntity, IEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public EvaluationTargetType TargetType { get; set; } 
    public Guid TargetId { get; set; } 

    public int Rating { get; set; } // 1 a 5
    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }

    // Navigation (opcional)
    public User User { get; set; } = null!;
}
