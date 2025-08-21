

namespace Application.UseCases.Establishments.GetEstablishmentSports;

public class GetEstablishmentSportsHandler : IQueryHandler<GetEstablishmentSportsQuery, GetEstablishmentSportsResponse>
{
    private readonly IEstablishmentsRepository _establishmentsRepository;
    
    public GetEstablishmentSportsHandler(IEstablishmentsRepository establishmentsRepository)
    {
        _establishmentsRepository = establishmentsRepository;
    }

    public async Task<Result<GetEstablishmentSportsResponse>> Handle(GetEstablishmentSportsQuery request, CancellationToken cancellationToken)
    {
        var sports = await _establishmentsRepository.GetSportsByEstablishmentIdAsync(request.EstablishmentId, cancellationToken);
        var response = new GetEstablishmentSportsResponse { Sports = sports.Select(s => new SportDto { Id = s.Id, Name = s.Name, Description = s.Description }) };

        return Result.Ok(response);
    }
}
