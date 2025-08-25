using Application.CQRS;

namespace Application.UseCases.Sports.GetAllSports;

public class GetAllSportsHandler : IQueryHandler<GetAllSportsQuery, GetAllSportsResponse>
{
    public readonly ISportService _sportService;

    public GetAllSportsHandler(ISportService sportService)
    {
        _sportService = sportService;
    }

    public async Task<Result<GetAllSportsResponse>> Handle(GetAllSportsQuery request, CancellationToken cancellationToken)
    {
        var sports = await _sportService.GetAllAsync(ct: cancellationToken);
        var response = new GetAllSportsResponse { Sports = sports.Select(s => new SportDto { Id = s.Id, Name = s.Name, Description = s.Description }) };

        return Result.Ok(response);
    }
}
