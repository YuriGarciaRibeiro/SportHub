using Application.CQRS;

namespace Application.UseCases.Customers.GetCustomerDetail;

public record GetCustomerDetailQuery(Guid CustomerId) : IQuery<CustomerDetailDto>;

public class CustomerDetailDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    // Métricas
    public int TotalReservations { get; set; }
    public decimal TotalSpent { get; set; }
    public DateTime? LastReservationAt { get; set; }

    // Quadras favoritas (top 3)
    public List<CourtFrequencyDto> FavoriteCourts { get; set; } = [];

    // Histórico de reservas (últimas 20)
    public List<CustomerReservationDto> RecentReservations { get; set; } = [];
}

public class CourtFrequencyDto
{
    public Guid CourtId { get; set; }
    public string CourtName { get; set; } = null!;
    public int Count { get; set; }
}

public class CustomerReservationDto
{
    public Guid Id { get; set; }
    public string CourtName { get; set; } = null!;
    public DateTime StartTimeUtc { get; set; }
    public DateTime EndTimeUtc { get; set; }
    public decimal Amount { get; set; }
}
