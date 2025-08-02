using Application.CQRS;

namespace Application.UseCases.Court.UpdateCourt;

public class UpdateCourtCommand : ICommand<UpdateCourtResponse>
{
    public Guid Id { get; set; }
    public Guid EstablishmentId { get; set; }

    public UpdateCourtRequest Request { get; set; } = null!;


}

public class UpdateCourtRequest
    {
        public string? Name { get; set; }
        public int? SlotDurationMinutes { get; set; }
        public int? MinBookingSlots { get; set; }
        public int? MaxBookingSlots { get; set; }
        public TimeOnly? OpeningTime { get; set; }
        public TimeOnly? ClosingTime { get; set; }
        public string? TimeZone { get; set; }
        public IEnumerable<Guid>? SportIds { get; set; }
    }