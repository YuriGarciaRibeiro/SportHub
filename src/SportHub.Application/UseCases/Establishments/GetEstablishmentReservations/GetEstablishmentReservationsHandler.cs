
using Application.Common.Interfaces;

namespace Application.UseCases.Establishments.GetEstablishmentReservations;

public class GetEstablishmentReservationsHandler : IQueryHandler<GetEstablishmentReservationsQuery, GetEstablishmentReservationsResponse>
{
    private readonly IEstablishmentsRepository _establishmentRepository;
    private readonly ICourtsRepository _courtsRepository;

    public GetEstablishmentReservationsHandler(IEstablishmentsRepository establishmentRepository, ICourtsRepository courtsRepository)
    {
        _establishmentRepository = establishmentRepository;
        _courtsRepository = courtsRepository;
    }

    public async Task<Result<GetEstablishmentReservationsResponse>> Handle(GetEstablishmentReservationsQuery request, CancellationToken cancellationToken)
    {
        var courtIds = await _courtsRepository.GetCourtIdsByEstablishmentIdAsync(request.EstablishmentId, cancellationToken);

        var reservations = await _establishmentRepository.GetReservationsByCourtsIdAsync(courtIds, request.Filter, cancellationToken);

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
