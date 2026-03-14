using Domain.Common;
using SportHub.Domain.Common;

namespace Domain.Entities;

public class Court : AuditEntity, IEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public decimal PricePerHour { get; set; } = 0;

    public int SlotDurationMinutes { get; set; } = 60;
    public int MinBookingSlots { get; set; } = 1;
    public int MaxBookingSlots { get; set; } = 4;
    public TimeOnly OpeningTime { get; set; } = new TimeOnly(8, 0);
    public TimeOnly ClosingTime { get; set; } = new TimeOnly(22, 0);

    public string TimeZone { get; set; } = "America/Maceio";
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public IEnumerable<Sport> Sports { get; set; } = new List<Sport>();
}
