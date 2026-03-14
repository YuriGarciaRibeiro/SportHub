using Application.CQRS;

namespace Application.UseCases.Admin.GetAdminStats;

public record GetAdminStatsQuery : IQuery<AdminStatsResponse>;

public record AdminStatsResponse(
    int TotalCourts,
    int TotalSports,
    int ReservationsToday
);
