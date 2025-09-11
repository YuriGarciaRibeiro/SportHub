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
    public TimeOnly OpeningTime { get; set; } = new TimeOnly(8, 0);
    public TimeOnly ClosingTime { get; set; } = new TimeOnly(22, 0);

    public decimal PricePerSlot { get; set; } = 0;

    public ICollection<Sport> Sports { get; set; } = new List<Sport>();

    public Establishment Establishment { get; set; } = null!;
}
