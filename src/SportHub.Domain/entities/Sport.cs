using Domain.Common;
using SportHub.Domain.Common;

namespace Domain.Entities;

public class Sport : AuditEntity, IEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string ImageUrl { get; set; } = null!;
    public ICollection<Court> Courts { get; set; } = new List<Court>();
}
