using Application.CQRS;

namespace Application.UseCases.Court.GetAvailability;

public class GetAvailabilityQuery : IQuery<GetAvailabilityResponse>
{
    public Guid CourtId { get; set; }
    public DateTime Date { get; set; }
}
