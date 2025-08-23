

namespace Application.UseCases.Establishments.GetEstablishmentSports;

public class GetEstablishmentSportsHandler : IQueryHandler<GetEstablishmentSportsQuery, GetEstablishmentSportsResponse>
{
    private readonly IEstablishmentService _establishmentService;
    
    public GetEstablishmentSportsHandler(IEstablishmentService establishmentService)
    {
        _establishmentService = establishmentService;
    }

    public async Task<Result<GetEstablishmentSportsResponse>> Handle(GetEstablishmentSportsQuery request, CancellationToken cancellationToken)
    {
        var sports = await _establishmentService.GetSportsByEstablishmentIdAsync(request.EstablishmentId, cancellationToken);
        var response = new GetEstablishmentSportsResponse { Sports = sports.Select(s => new SportDto { Id = s.Id, Name = s.Name, Description = s.Description }) };

        return Result.Ok(response);
    }
}
