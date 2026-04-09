using Application.Common.Interfaces;
using Application.CQRS;

namespace Application.UseCases.Reservations.GetActiveReservation;

public class GetActiveReservationHandler : IQueryHandler<GetActiveReservationQuery, ActiveReservationResponse?>
{
    private readonly IReservationRepository _reservationRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetActiveReservationHandler(
        IReservationRepository reservationRepository,
        ICurrentUserService currentUserService)
    {
        _reservationRepository = reservationRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<ActiveReservationResponse?>> Handle(GetActiveReservationQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        var reservation = await _reservationRepository.GetActiveByUserAsync(userId, DateTime.UtcNow, cancellationToken);
        if (reservation is null)
            return Result.Ok<ActiveReservationResponse?>(null);

        var response = new ActiveReservationResponse(
            reservation.Id,
            reservation.Court.Name,
            reservation.StartTimeUtc,
            reservation.EndTimeUtc);

        return Result.Ok<ActiveReservationResponse?>(response);
    }
}
