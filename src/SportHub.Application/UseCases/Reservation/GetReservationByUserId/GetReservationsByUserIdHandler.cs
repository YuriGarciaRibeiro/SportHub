namespace SportHub.Application.UseCases.Reservation.GetReservationByUserId;

public class GetReservationsByUserIdHandler : IQueryHandler<GetReservationsByUserIdQuery, GetReservationsByUserIdResponse>
{
    private readonly IReservationRepository _reservationRepository;

    public GetReservationsByUserIdHandler(IReservationRepository reservationRepository)
    {
        _reservationRepository = reservationRepository;
    }

    public async Task<Result<GetReservationsByUserIdResponse>> Handle(GetReservationsByUserIdQuery request, CancellationToken cancellationToken)
    {
        var (items, total) = await _reservationRepository.GetReservationsByUserIdAsync(request.UserId, request.Page, request.PageSize, cancellationToken);

        var response = new GetReservationsByUserIdResponse
        {
            Items = items.Select(r => new ReservationResponse
            {
                ReservationId = r.Id,
                StartTimeUtc = r.StartTimeUtc,
                EndTimeUtc = r.EndTimeUtc,
                TotalPrice = r.TotalPrice,
                SlotsBooked = r.SlotsBooked,
                Establishment = new EstablishmentResponse
                {
                    EstablishmentId = r.EstablishmentId,
                    Name = r.EstablishmentName,
                    Court = new CourtResponse
                    {
                        CourtId = r.CourtId,
                        Name = r.CourtName,
                    }
                },
            }),
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize
        };

        return Result.Ok(response);
    }
}
