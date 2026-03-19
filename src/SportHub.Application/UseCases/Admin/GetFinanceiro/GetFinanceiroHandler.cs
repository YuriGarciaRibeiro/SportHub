using Application.Common.Interfaces;
using Application.CQRS;
using FluentResults;

namespace Application.UseCases.Admin.GetFinanceiro;

public class GetFinanceiroHandler : IQueryHandler<GetFinanceiroQuery, FinanceiroResponse>
{
    private readonly IReservationRepository _reservationRepo;

    public GetFinanceiroHandler(IReservationRepository reservationRepo)
    {
        _reservationRepo = reservationRepo;
    }

    public async Task<Result<FinanceiroResponse>> Handle(GetFinanceiroQuery request, CancellationToken ct)
    {
        var brazilTz = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

        // Período selecionado
        var periodStartLocal = new DateTime(request.Year, request.Month, 1);
        var periodStartUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(periodStartLocal, DateTimeKind.Unspecified), brazilTz);
        var periodEndUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(periodStartLocal.AddMonths(1), DateTimeKind.Unspecified), brazilTz);

        // Período anterior (mesmo número de dias)
        var prevStartUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(periodStartLocal.AddMonths(-1), DateTimeKind.Unspecified), brazilTz);
        var prevEndUtc = periodStartUtc;

        // Últimos 6 meses para o gráfico mensal
        var sixMonthsStartLocal = periodStartLocal.AddMonths(-5);
        var sixMonthsStartUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(sixMonthsStartLocal, DateTimeKind.Unspecified), brazilTz);

        // Hoje (para projeção)
        var todayLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, brazilTz).Date;
        var isCurrentMonth = todayLocal.Year == request.Year && todayLocal.Month == request.Month;

        // Queries sequenciais — EF Core não suporta operações concorrentes no mesmo DbContext
        var dailyRevenue = await _reservationRepo.GetDailyRevenueAsync(periodStartUtc, periodEndUtc, ct);
        var prevRevenue = await _reservationRepo.GetDailyRevenueAsync(prevStartUtc, prevEndUtc, ct);
        var allDaily = await _reservationRepo.GetDailyRevenueAsync(sixMonthsStartUtc, periodEndUtc, ct);
        var reservationsCount = await _reservationRepo.CountByPeriodAsync(periodStartUtc, periodEndUtc, ct);
        var reservationsLastPeriod = await _reservationRepo.CountByPeriodAsync(prevStartUtc, prevEndUtc, ct);
        var cancellations = await _reservationRepo.GetCancellationStatsAsync(periodStartUtc, periodEndUtc, ct);
        var cancellationsLast = await _reservationRepo.GetCancellationStatsAsync(prevStartUtc, prevEndUtc, ct);
        var courtRevenue = await _reservationRepo.GetRevenueByCourtAsync(periodStartUtc, periodEndUtc, ct);
        var topCustomers = await _reservationRepo.GetTopCustomersAsync(periodStartUtc, periodEndUtc, 5, ct);

        var revenueThisPeriod = dailyRevenue.Sum(d => d.Revenue);
        var revenueLastPeriod = prevRevenue.Sum(d => d.Revenue);

        // Projeção: só faz sentido no mês atual
        decimal revenueProjected = 0;
        if (isCurrentMonth && todayLocal.Day > 0)
        {
            var daysInMonth = DateTime.DaysInMonth(request.Year, request.Month);
            revenueProjected = revenueThisPeriod / todayLocal.Day * daysInMonth;
        }

        var averageTicket = reservationsCount > 0 ? revenueThisPeriod / reservationsCount : 0m;
        var averageTicketLast = reservationsLastPeriod > 0 ? revenueLastPeriod / reservationsLastPeriod : 0m;

        // Série diária do período
        var dailySeries = dailyRevenue
            .Select(d => new DailyRevenueItemDto(d.Date.ToString("yyyy-MM-dd"), d.Revenue))
            .ToList();

        // Série mensal dos últimos 6 meses
        var monthlySeries = allDaily
            .GroupBy(d => new { d.Date.Year, d.Date.Month })
            .Select(g => new MonthlyRevenueItemDto(
                new DateTime(g.Key.Year, g.Key.Month, 1).ToString("yyyy-MM"),
                g.Sum(d => d.Revenue)))
            .OrderBy(m => m.Month)
            .ToList();

        var revenueByCourt = courtRevenue
            .Select(c => new RevenueByCourtDto(c.CourtId, c.CourtName, c.Revenue, c.Reservations))
            .ToList();

        var top = topCustomers
            .Select(c => new TopCustomerDto(c.UserId, c.Name, c.Email, c.TotalSpent, c.Reservations))
            .ToList();

        return Result.Ok(new FinanceiroResponse(
            revenueThisPeriod,
            revenueLastPeriod,
            revenueProjected,
            reservationsCount,
            reservationsLastPeriod,
            cancellations.Count,
            cancellationsLast.Count,
            cancellations.Revenue,
            averageTicket,
            averageTicketLast,
            dailySeries,
            monthlySeries,
            revenueByCourt,
            top
        ));
    }
}
