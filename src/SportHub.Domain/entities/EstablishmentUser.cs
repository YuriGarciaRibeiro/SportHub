using Domain.Enums;
using SportHub.Domain.Common;

namespace Domain.Entities;

public class EstablishmentUser : AuditEntity
    {
    public Guid EstablishmentId { get; set; }
    public Establishment Establishment { get; set; } = null!;

    public Guid UserId { get; set; }
    public EstablishmentRole Role { get; set; } = EstablishmentRole.Owner;
}
