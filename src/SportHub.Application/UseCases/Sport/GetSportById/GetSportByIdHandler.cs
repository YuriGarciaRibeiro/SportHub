using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.CQRS;
using Application.UseCases.Sport.GetAllSports;

namespace Application.UseCases.Sport.GetSportById;

public class GetSportByIdHandler : IQueryHandler<GetSportByIdQuery, SportSummaryResponse>
{
    private readonly ISportsRepository _sportsRepository;

    public GetSportByIdHandler(ISportsRepository sportsRepository)
    {
        _sportsRepository = sportsRepository;
    }

    public async Task<Result<SportSummaryResponse>> Handle(GetSportByIdQuery request, CancellationToken cancellationToken)
    {
        var sport = await _sportsRepository.GetByIdAsync(request.Id);

        if (sport is null)
            return Result.Fail(new NotFound($"Esporte com ID {request.Id} não encontrado."));

        return Result.Ok(new SportSummaryResponse(sport.Id, sport.Name, sport.Description, sport.ImageUrl));
    }
}
