namespace Application.UseCases.Establishments.GetEstablishmentReservations;

public class GetEstablishmentReservationsResponse
{
    public IEnumerable<ReservationDto> Reservations { get; set; } = Enumerable.Empty<ReservationDto>();
}

public class ReservationDto
{
    public Guid Id { get; set; }
    public DateTime StartTimeUtc { get; set; }
    public DateTime EndTimeUtc { get; set; }
    public CourtDto Court { get; set; } = null!;
    public UserDto User { get; set; } = null!;
}

public class CourtDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class UserDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

