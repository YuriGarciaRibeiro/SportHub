
using Application.Common.QueryFilters;

namespace Application.UseCases.Court.GetCourts;

public class GetCourtsHandler : ICommandHandler<GetCourtsQuery, GetCourtsResponse>
{
    private readonly ICourtService _courtService;

    public GetCourtsHandler(ICourtService courtService)
    {
        _courtService = courtService;
    }

    public async Task<Result<GetCourtsResponse>> Handle(GetCourtsQuery request, CancellationToken cancellationToken)
    {
        var courts = await _courtService.GetByFilterAsync(request.Filter, cancellationToken);
        var courtDtos = courts.Select(c => new CourtDto
        {
            Id = c.Id,
            Name = c.Name,
            OpeningTime = c.OpeningTime,
            ClosingTime = c.ClosingTime,
            Establishment = new EstablishmentDto
            {
                Id = c.Establishment.Id,
                Name = c.Establishment.Name
            },
            Sports = c.Sports.Select(s => new SportDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description
            })
        });

        return Result.Ok(new GetCourtsResponse
        {
            Courts = courtDtos
        });

    }
}
