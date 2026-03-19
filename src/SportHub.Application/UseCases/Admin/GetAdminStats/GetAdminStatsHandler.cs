using Application.Common.Interfaces;
using Application.CQRS;
using FluentResults;

namespace Application.UseCases.Admin.GetAdminStats;

public class GetAdminStatsHandler : IQueryHandler<GetAdminStatsQuery, AdminStatsResponse>
{
    private readonly ICourtsRepository _courtsRepo;
    private readonly ISportsRepository _sportsRepo;
    private readonly IReservationRepository _reservationRepo;
    private readonly IUsersRepository _usersRepo;

    public GetAdminStatsHandler(
        ICourtsRepository courtsRepo,
        ISportsRepository sportsRepo,
        IReservationRepository reservationRepo,
        IUsersRepository usersRepo)
    {
        _courtsRepo = courtsRepo;
        _sportsRepo = sportsRepo;
        _reservationRepo = reservationRepo;
        _usersRepo = usersRepo;
    }

    public async Task<Result<AdminStatsResponse>> Handle(
        GetAdminStatsQuery request, CancellationToken ct)
    {
        var brazilTz = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
        var todayLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, brazilTz).Date;
        var todayStartUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(todayLocal, DateTimeKind.Unspecified), brazilTz);
        var last30DaysStartUtc = todayStartUtc.AddDays(-29);

        // mês atual: do dia 1 até amanhã (inclui hoje)
        var thisMonthStartLocal = new DateTime(todayLocal.Year, todayLocal.Month, 1);
        var thisMonthStartUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(thisMonthStartLocal, DateTimeKind.Unspecified), brazilTz);
        var thisMonthEndUtc = todayStartUtc.AddDays(1);

        // mês passado: do dia 1 ao último dia
        var lastMonthStartLocal = thisMonthStartLocal.AddMonths(-1);
        var lastMonthStartUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(lastMonthStartLocal, DateTimeKind.Unspecified), brazilTz);
        var lastMonthEndUtc = thisMonthStartUtc;

        var courts = await _courtsRepo.GetAllAsync();
        var totalCourts = courts.Count;

        var sports = await _sportsRepo.GetAllAsync();
        var totalSports = sports.Count;

        var totalReservations = await _reservationRepo.CountByDayAsync(todayStartUtc);
        var totalRevenue = await _reservationRepo.GetTotalRevenueByDayAsync(todayStartUtc);

        var usersPage = await _usersRepo.GetPagedAsync(page: 1, pageSize: 1);
        var totalUsers = usersPage.TotalCount;

        var dailyRevenue = await _reservationRepo.GetDailyRevenueAsync(last30DaysStartUtc, todayStartUtc.AddDays(1), ct);
        var revenueLastDays = dailyRevenue
            .Select(d => new DailyRevenueDto(d.Date.ToString("yyyy-MM-dd"), d.Revenue))
            .ToList();

        var thisMonthRevenue = await _reservationRepo.GetDailyRevenueAsync(thisMonthStartUtc, thisMonthEndUtc, ct);
        var revenueThisMonth = thisMonthRevenue.Sum(d => d.Revenue);

        var lastMonthRevenue = await _reservationRepo.GetDailyRevenueAsync(lastMonthStartUtc, lastMonthEndUtc, ct);
        var revenueLastMonth = lastMonthRevenue.Sum(d => d.Revenue);

        // projeção: ritmo diário médio × total de dias do mês
        var daysPassedThisMonth = todayLocal.Day;
        var daysInMonth = DateTime.DaysInMonth(todayLocal.Year, todayLocal.Month);
        var revenueProjectedMonth = daysPassedThisMonth > 0
            ? revenueThisMonth / daysPassedThisMonth * daysInMonth
            : 0m;

        var courtOccupancy = await _reservationRepo.GetCourtOccupancyTodayAsync(todayStartUtc, ct);
        var courtOccupancyToday = courtOccupancy
            .Select(c => new CourtOccupancyDto(c.CourtId, c.CourtName, c.TotalSlots, c.BookedSlots))
            .ToList();

        return Result.Ok(new AdminStatsResponse(
            totalCourts,
            totalSports,
            totalReservations,
            totalUsers,
            totalRevenue,
            revenueThisMonth,
            revenueLastMonth,
            revenueProjectedMonth,
            revenueLastDays,
            courtOccupancyToday
        ));
    }
}
