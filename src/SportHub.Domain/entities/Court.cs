using Domain.Common;

namespace Domain.Entities;

public class Court : AuditEntity, IEntity
{
    public Guid Id { get; set; }
    public Guid EstablishmentId { get; set; }

    public string Name { get; set; } = null!;

    public int SlotDurationMinutes { get; set; } = 30;
    public int MinBookingSlots { get; set; } = 1;
    public int MaxBookingSlots { get; set; } = 4;

    public string TimeZone { get; set; } = "America/Maceio";
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public IEnumerable<Sport> Sports { get; set; } = null!;

    // Navegação
    public Establishment Establishment { get; set; } = null!;
}
