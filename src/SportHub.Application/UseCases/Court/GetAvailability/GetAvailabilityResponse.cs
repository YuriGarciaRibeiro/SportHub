

namespace Application.UseCases.Court.GetAvailability;

public class GetAvailabilityResponse
{
    public DateTime Date { get; set; }

    public List<DateTime> AvailableSlotsUtc { get; set; } = new();
}
