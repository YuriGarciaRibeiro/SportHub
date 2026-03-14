using Application.Common.Interfaces;
using Application.CQRS;

namespace Application.UseCases.Sport.GetAllSports;

public class GetAllSportsHandler : IQueryHandler<GetAllSportsQuery, List<SportSummaryResponse>>
{
    private readonly ISportsRepository _sportsRepository;

    public GetAllSportsHandler(ISportsRepository sportsRepository)
    {
        _sportsRepository = sportsRepository;
    }

    public async Task<Result<List<SportSummaryResponse>>> Handle(GetAllSportsQuery request, CancellationToken cancellationToken)
    {
        var sports = await _sportsRepository.GetAllAsync();

        var response = sports
            .Select(s => new SportSummaryResponse(s.Id, s.Name, s.Description, s.ImageUrl))
            .ToList();

        return Result.Ok(response);
    }
}
