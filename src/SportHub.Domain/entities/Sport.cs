using Domain.Common;

namespace Domain.Entities;

public class Sport : AuditEntity, IEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string ImageUrl { get; set; } = null!;
    public ICollection<Court> Courts { get; set; } = new List<Court>();
    public ICollection<Establishment> Establishments { get; set; } = new List<Establishment>();
}
