using Domain.Common;
using SportHub.Domain.Common;

namespace Domain.Entities;

public class Location : AuditEntity, IEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }

    public string Name { get; set; } = null!;
    public Address? Address { get; set; }
    public string? Phone { get; set; }
    public List<DailySchedule> BusinessHours { get; set; } = new();
    public bool IsDefault { get; set; }

    // Redes sociais da unidade (opcionais — podem diferir da marca global)
    public string? InstagramUrl { get; set; }
    public string? FacebookUrl { get; set; }
    public string? WhatsappNumber { get; set; }

    public ICollection<Court> Courts { get; set; } = new List<Court>();
}
