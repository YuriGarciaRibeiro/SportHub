using Application.Common.Interfaces;
using Application.CQRS;

namespace Application.UseCases.Reservations.GetMyReservations;

public class GetMyReservationsHandler : IQueryHandler<GetMyReservationsQuery, List<ReservationResponse>>
{
    private readonly IReservationRepository _reservationRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetMyReservationsHandler(
        IReservationRepository reservationRepository,
        ICurrentUserService currentUserService)
    {
        _reservationRepository = reservationRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<ReservationResponse>>> Handle(GetMyReservationsQuery request, CancellationToken cancellationToken)
    {
        var reservations = await _reservationRepository.GetByUserAsync(_currentUserService.UserId);

        var response = reservations
            .Select(r => new ReservationResponse(
                r.Id,
                r.CourtId,
                r.Court.Name,
                r.StartTimeUtc,
                r.EndTimeUtc))
            .ToList();

        return Result.Ok(response);
    }
}
