using Application.CQRS;

namespace Application.UseCases.Admin.GetAdminStats;

public record GetAdminStatsQuery : IQuery<AdminStatsResponse>;

public record AdminStatsResponse(
    int TotalCourts,
    int TotalSports,
    int TotalReservations,
    int TotalUsers,
    decimal TotalRevenue,
    decimal RevenueThisMonth,
    decimal RevenueLastMonth,
    decimal RevenueProjectedMonth,
    List<DailyRevenueDto> RevenueLastDays,
    List<CourtOccupancyDto> CourtOccupancyToday
);

public record DailyRevenueDto(string Date, decimal Revenue);

public record CourtOccupancyDto(Guid CourtId, string CourtName, int TotalSlots, int BookedSlots);
