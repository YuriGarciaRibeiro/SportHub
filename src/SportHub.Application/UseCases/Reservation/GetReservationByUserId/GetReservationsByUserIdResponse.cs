namespace SportHub.Application.UseCases.Reservation.GetReservationByUserId;

public class GetReservationsByUserIdResponse
{
    public IEnumerable<ReservationResponse> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
}

public class ReservationResponse
{
    public Guid ReservationId { get; init; }
    public DateTime StartTimeUtc { get; init; }
    public DateTime EndTimeUtc { get; init; }
    public decimal TotalPrice { get; init; }
    public int SlotsBooked { get; init; }

    public EstablishmentResponse Establishment { get; init; } = new();
}
public class EstablishmentResponse
{
    public Guid EstablishmentId { get; init; }
    public string Name { get; init; } = string.Empty;
    public CourtResponse Court { get; init; } = new();
}

public class CourtResponse
{
    public Guid CourtId { get; init; }
    public string Name { get; init; } = string.Empty;
}

