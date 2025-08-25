
using Application.Common.Errors;

namespace Application.UseCases.Court.GetCourt;

public class GetCourtHandler : IQueryHandler<GetCourtQuery, GetCourtResponse>
{   
    private readonly ICourtService _courtService;

    public GetCourtHandler(ICourtService courtService)
    {
        _courtService = courtService;
    }

    public async Task<Result<GetCourtResponse>> Handle(GetCourtQuery request, CancellationToken cancellationToken)
    {
        var court = await _courtService.GetCompleteByIdAsync(request.CourtId, cancellationToken);
        if (court == null)
        {
            return Result.Fail(new NotFound($"Court with ID {request.CourtId} not found."));
        }
        

        var response = new GetCourtResponse
        {
            Id = court.Id,
            Name = court.Name,
            OpeningTime = court.OpeningTime,
            ClosingTime = court.ClosingTime,
            Sports = court.Sports.Select(s => new SportDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description
            }),
            Establishment = new EstablishmentDto
            {
                Id = court.Establishment.Id,
                Name = court.Establishment.Name,
                Description = court.Establishment.Description,
                ImageUrl = court.Establishment.ImageUrl
            }
        };

        return Result.Ok(response);
    }
}
