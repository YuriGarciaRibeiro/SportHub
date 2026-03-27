using Domain.Common;
using SportHub.Domain.Common;

namespace Domain.Entities;

public class Reservation : TenantEntity, IEntity
{
    public Guid Id { get; set; }
    public Guid CourtId { get; set; }
    public Guid UserId { get; set; }

    public DateTime StartTimeUtc { get; set; }
    public DateTime EndTimeUtc { get; set; }

    public decimal TotalPrice { get; set; }
    public bool IsPeakHours { get; set; }
    public decimal PricePerHour { get; set; }
    public int NormalSlots { get; set; }
    public int PeakSlots { get; set; }
    public decimal NormalSubtotal { get; set; }
    public decimal PeakSubtotal { get; set; }
    public decimal? NormalPricePerSlot { get; set; }
    public decimal? PeakPricePerSlot { get; set; }

    public Court Court { get; set; } = null!;
    public User User { get; set; } = null!;
    public User CreatedByUser { get; set; } = null!;
}
