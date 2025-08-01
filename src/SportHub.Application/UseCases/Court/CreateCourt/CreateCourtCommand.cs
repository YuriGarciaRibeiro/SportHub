using Application.CQRS;

namespace Application.UseCases.Court.CreateCourt;

public class CreateCourtCommand : ICommand
{

    public CreateCourtCommand(Guid establishmentId, CourtRequest court)
    {
        EstablishmentId = establishmentId;
        Court = court;
    }

    public Guid EstablishmentId { get; set; }
    public CourtRequest Court { get; set; } = new CourtRequest();
    
}

public class CourtRequest
{
    public string Name { get; set; } = null!;
    public string SportType { get; set; } = null!;
    public int SlotDurationMinutes { get; set; } = 30;
    public int MinBookingSlots { get; set; } = 1;
    public int MaxBookingSlots { get; set; } = 4;
    public TimeOnly OpeningTime { get; set; } = new TimeOnly(8, 0);
    public TimeOnly ClosingTime { get; set; } = new TimeOnly(22, 0);
    public string TimeZone { get; set; } = "America/Maceio";
    public IEnumerable<Guid> Sports { get; set; } = new List<Guid>();
}
