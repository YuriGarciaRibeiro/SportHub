using Domain.Entities;

namespace Application.UseCases.Court.UpdateCourt;

public class UpdateCourtResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public int SlotDurationMinutes { get; set; }
    public int MinBookingSlots { get; set; }
    public int MaxBookingSlots { get; set; }
    public TimeOnly OpeningTime { get; set; }
    public TimeOnly ClosingTime { get; set; }
    public string TimeZone { get; set; } = null!;
    public DateTime CreatedAtUtc { get; set; }
    public IEnumerable<Sport> Sports { get; set; } = null!;
}


