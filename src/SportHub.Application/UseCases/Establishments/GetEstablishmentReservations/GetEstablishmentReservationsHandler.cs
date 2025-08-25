

namespace Application.UseCases.Establishments.GetEstablishmentReservations;

public class GetEstablishmentReservationsHandler : IQueryHandler<GetEstablishmentReservationsQuery, GetEstablishmentReservationsResponse>
{
    private readonly IEstablishmentService _establishmentService;
    private readonly ICourtService _courtService;

    public GetEstablishmentReservationsHandler(IEstablishmentService establishmentService, ICourtService courtService)
    {
        _establishmentService = establishmentService;
        _courtService = courtService;
    }

    public async Task<Result<GetEstablishmentReservationsResponse>> Handle(GetEstablishmentReservationsQuery request, CancellationToken cancellationToken)
    {
        var courtIds = await _courtService.GetCourtIdsByEstablishmentIdAsync(request.EstablishmentId, cancellationToken);

        var reservations = await _establishmentService.GetReservationsByCourtsIdAsync(courtIds, request.Filter, cancellationToken);

        var response = new GetEstablishmentReservationsResponse
        {
            Reservations = reservations.Select(r => new ReservationDto
            {
                Id = r.Id,
                EndTimeUtc = r.EndTimeUtc,
                StartTimeUtc = r.StartTimeUtc,
                Court = new CourtDto
                {
                    Id = r.CourtId,
                    Name = r.CourtName
                },
                User = new UserDto
                {
                    Id = r.UserId,
                    Name = r.UserName,
                    Email = r.UserEmail
                }

            })
        };

        return Result.Ok(response);
    }
}
