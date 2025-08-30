using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class Favorite : AuditEntity, IEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public FavoriteType EntityType { get; set; }
    public Guid EntityId { get; set; }

    public User User { get; set; } = null!;
    
}
