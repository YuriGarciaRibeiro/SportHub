

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
                    Id = r.Court.Id,
                    Name = r.Court.Name
                },
                User = new UserDto
                {
                    Id = r.User.Id,
                    Name = r.User.FullName,
                    Email = r.User.Email
                }

            })
        };

        return Result.Ok(response);
    }
}
