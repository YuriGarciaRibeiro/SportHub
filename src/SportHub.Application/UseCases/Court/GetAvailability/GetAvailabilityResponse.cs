

namespace Application.UseCases.Court.GetAvailability;

public class GetAvailabilityResponse
{
    public DateTime Date { get; set; }

    public IEnumerable<DateTime> AvailableSlotsUtc { get; set; } = new List<DateTime>();
}
