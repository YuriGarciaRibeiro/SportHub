using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.CQRS;
using Application.UseCases.Reservations.GetMyReservations;

namespace Application.UseCases.Reservations.GetCourtReservations;

public class GetCourtReservationsHandler : IQueryHandler<GetCourtReservationsQuery, List<ReservationResponse>>
{
    private readonly IReservationRepository _reservationRepository;
    private readonly ICourtsRepository _courtsRepository;

    public GetCourtReservationsHandler(
        IReservationRepository reservationRepository,
        ICourtsRepository courtsRepository)
    {
        _reservationRepository = reservationRepository;
        _courtsRepository = courtsRepository;
    }

    public async Task<Result<List<ReservationResponse>>> Handle(GetCourtReservationsQuery request, CancellationToken cancellationToken)
    {
        var courtExists = await _courtsRepository.ExistsAsync(request.CourtId);
        if (!courtExists)
            return Result.Fail(new NotFound($"Quadra com ID {request.CourtId} não encontrada."));

        var reservations = await _reservationRepository.GetByCourtAsync(request.CourtId, request.Date);

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
