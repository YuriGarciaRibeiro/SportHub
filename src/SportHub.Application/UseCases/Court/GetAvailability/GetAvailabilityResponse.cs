

namespace Application.UseCases.Court.GetAvailability;

public class GetAvailabilityResponse
{
    public DateTime Date { get; set; }

    /// <summary>Mantido para compatibilidade com clientes existentes.</summary>
    public List<DateTime> AvailableSlotsUtc { get; set; } = new();

    public List<SlotInfo> SlotsUtc { get; set; } = new();
}

public class SlotInfo
{
    public DateTime StartUtc { get; set; }
    public bool IsAvailable { get; set; }
}
