using Domain.Common;

namespace Domain.Entities;

public class Reservation : AuditEntity, IEntity
{
    public Guid Id { get; set; }
    public Guid CourtId { get; set; }
    public Guid UserId { get; set; }

    public DateTime StartTimeUtc { get; set; }
    public DateTime EndTimeUtc { get; set; }

    public Court Court { get; set; } = null!;
    public User User { get; set; } = null!;
}
