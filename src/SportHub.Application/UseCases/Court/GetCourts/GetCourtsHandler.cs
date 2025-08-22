
using Application.Common.Interfaces;
using Application.Common.QueryFilters;

namespace Application.UseCases.Court.GetCourts;

public class GetCourtsHandler : ICommandHandler<GetCourtsQuery, GetCourtsResponse>
{
    private readonly ICourtsRepository _courtRepository;

    public GetCourtsHandler(ICourtsRepository courtRepository)
    {
        _courtRepository = courtRepository;
    }

    public async Task<Result<GetCourtsResponse>> Handle(GetCourtsQuery request, CancellationToken cancellationToken)
    {
        var courts = await _courtRepository.GetByFilterAsync(request.Filter);
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
