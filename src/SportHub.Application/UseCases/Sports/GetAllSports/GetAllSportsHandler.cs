using Application.Common.Interfaces;
using Application.CQRS;

namespace Application.UseCases.Sports.GetAllSports;

public class GetAllSportsHandler : IQueryHandler<GetAllSportsQuery, Result<GetAllSportsResponse>>
{
    public readonly ISportsRepository _sportsRepository;

    public GetAllSportsHandler(ISportsRepository sportsRepository)
    {
        _sportsRepository = sportsRepository;
    }

    public async Task<Result<Result<GetAllSportsResponse>>> Handle(GetAllSportsQuery request, CancellationToken cancellationToken)
    {
        var sports = await _sportsRepository.GetAllAsync();
        var response = new GetAllSportsResponse { Sports = sports.Select(s => new SportDto { Id = s.Id, Name = s.Name, Description = s.Description }) };

        return Result.Ok(response);
    }
}
