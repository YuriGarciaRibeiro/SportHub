using Application.Common.Interfaces;
using Application.CQRS;
using FluentResults;

namespace Application.UseCases.Admin.GetAdminStats;

public class GetAdminStatsHandler : IQueryHandler<GetAdminStatsQuery, AdminStatsResponse>
{
    private readonly ICourtsRepository _courtsRepo;
    private readonly ISportsRepository _sportsRepo;
    private readonly IReservationRepository _reservationRepo;

    public GetAdminStatsHandler(
        ICourtsRepository courtsRepo,
        ISportsRepository sportsRepo,
        IReservationRepository reservationRepo)
    {
        _courtsRepo = courtsRepo;
        _sportsRepo = sportsRepo;
        _reservationRepo = reservationRepo;
    }

    public async Task<Result<AdminStatsResponse>> Handle(
        GetAdminStatsQuery request, CancellationToken ct)
    {
        var today = DateTime.UtcNow.Date;

        var courts = await _courtsRepo.GetAllAsync();
        var courtList = courts.ToList();
        var totalCourts = courtList.Count;

        var sports = await _sportsRepo.GetAllAsync();
        var totalSports = sports.Count();

        // Reservas de hoje em todas as quadras
        int reservationsToday = 0;
        foreach (var court in courtList)
        {
            var reservations = await _reservationRepo.GetByCourtAndDayAsync(court.Id, today);
            reservationsToday += reservations.Count;
        }

        return Result.Ok(new AdminStatsResponse(
            totalCourts,
            totalSports,
            reservationsToday
        ));
    }
}
