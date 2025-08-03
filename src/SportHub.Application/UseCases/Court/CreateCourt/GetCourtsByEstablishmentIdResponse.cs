namespace Application.UseCases.Court.CreateCourt;


public class GetCourtResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SportType { get; set; } = string.Empty;
    public int SlotDurationMinutes { get; set; }
    public int MinBookingSlots { get; set; }
    public int MaxBookingSlots { get; set; }
    public TimeOnly OpeningTime { get; set; }
    public TimeOnly ClosingTime { get; set; }
    public string TimeZone { get; set; } = "America/Maceio";
    public List<SportResponse> Sports { get; set; } = new List<SportResponse>();
}

public class SportResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}