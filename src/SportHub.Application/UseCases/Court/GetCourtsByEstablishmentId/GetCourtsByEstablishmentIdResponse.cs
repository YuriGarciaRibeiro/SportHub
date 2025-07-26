namespace Application.UseCases.Court.GetCourtsByEstablishmentId;

public class GetCourtsByEstablishmentIdResponse
{
    public Guid EstablishmentId { get; set; }
    public List<CourtResponse> Courts { get; set; } = new List<CourtResponse>();
}

public class CourtResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SportType { get; set; } = string.Empty;
    public int SlotDurationMinutes { get; set; }
    public int MinBookingSlots { get; set; }
    public int MaxBookingSlots { get; set; }
    public string TimeZone { get; set; } = "America/Maceio";
    public List<SportResponse> Sports { get; set; } = new List<SportResponse>();
}

public class SportResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}