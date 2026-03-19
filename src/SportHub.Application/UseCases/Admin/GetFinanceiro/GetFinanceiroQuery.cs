using Application.CQRS;

namespace Application.UseCases.Admin.GetFinanceiro;

public record GetFinanceiroQuery(int Year, int Month) : IQuery<FinanceiroResponse>;

public record FinanceiroResponse(
    // Receita do período
    decimal RevenueThisPeriod,
    decimal RevenueLastPeriod,
    decimal RevenueProjected,

    // Reservas
    int ReservationsCount,
    int ReservationsLastPeriod,
    int CancellationsCount,
    int CancellationsLastPeriod,
    decimal CancelledRevenue,

    // Ticket médio
    decimal AverageTicket,
    decimal AverageTicketLastPeriod,

    // Séries temporais
    List<DailyRevenueItemDto> DailyRevenue,
    List<MonthlyRevenueItemDto> MonthlyRevenue,

    // Receita por quadra
    List<RevenueByCourtDto> RevenueByCourt,

    // Top clientes
    List<TopCustomerDto> TopCustomers
);

public record DailyRevenueItemDto(string Date, decimal Revenue);
public record MonthlyRevenueItemDto(string Month, decimal Revenue);
public record RevenueByCourtDto(Guid CourtId, string CourtName, decimal Revenue, int Reservations);
public record TopCustomerDto(Guid UserId, string Name, string Email, decimal TotalSpent, int Reservations);
