using Application.Common.Interfaces;
using Application.CQRS;
using Application.UseCases.Court.GetCourtById;
using FluentResults;

namespace Application.UseCases.Court.GetAllCourts;

public class GetAllCourtsHandler : IQueryHandler<GetAllCourtsQuery, List<CourtPublicResponse>>
{
    private readonly ICourtsRepository _courtsRepo;

    public GetAllCourtsHandler(ICourtsRepository courtsRepo)
    {
        _courtsRepo = courtsRepo;
    }

    public async Task<Result<List<CourtPublicResponse>>> Handle(
        GetAllCourtsQuery request, CancellationToken ct)
    {
        var courts = await _courtsRepo.GetAllAsync();
        var response = courts.Select(c => new CourtPublicResponse(
            c.Id,
            c.Name,
            c.ImageUrl,
            c.PricePerHour,
            c.SlotDurationMinutes,
            c.OpeningTime.ToString("HH:mm"),
            c.ClosingTime.ToString("HH:mm"),
            c.Sports.Select(s => new SportSummary(s.Id, s.Name)).ToList()
        )).ToList();

        return Result.Ok(response);
    }
}
