namespace Domain.Entities;

public class Court
{
    public Guid Id { get; set; }
    public Guid EstablishmentId { get; set; }

    public string Name { get; set; } = null!;
    public string SportType { get; set; } = null!;

    public int SlotDurationMinutes { get; set; } = 30;
    public int MinBookingSlots { get; set; } = 1;
    public int MaxBookingSlots { get; set; } = 4;

    public string TimeZone { get; set; } = "America/Maceio";
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    // Navegação
    public Establishment Establishment { get; set; } = null!;
}
