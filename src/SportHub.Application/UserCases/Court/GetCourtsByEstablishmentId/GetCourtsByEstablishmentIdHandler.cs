using Application.CQRS;
using Microsoft.Extensions.Logging;

namespace Application.UserCases.Court.GetCourtsByEstablishmentId;

public class GetCourtsByEstablishmentIdQueryHandler : IQueryHandler<GetCourtsByEstablishmentIdQuery, GetCourtsByEstablishmentIdResponse>
{
    private readonly ICourtsRepository _courtsRepository;
    private readonly ILogger<GetCourtsByEstablishmentIdQueryHandler> _logger;

    public GetCourtsByEstablishmentIdQueryHandler(ICourtsRepository courtsRepository, ILogger<GetCourtsByEstablishmentIdQueryHandler> logger)
    {
        _courtsRepository = courtsRepository;
        _logger = logger;
    }

    public async Task<Result<GetCourtsByEstablishmentIdResponse>> Handle(GetCourtsByEstablishmentIdQuery request, CancellationToken cancellationToken)
    {
        var courts = await _courtsRepository.GetByEstablishmentIdAsync(request.EstablishmentId);
        if (courts == null || !courts.Any())
        {
            return Result.Fail<GetCourtsByEstablishmentIdResponse>("No courts found for this establishment.");
        }

        var response = new GetCourtsByEstablishmentIdResponse
        {
            EstablishmentId = request.EstablishmentId,
            Courts = courts.Select(c => new CourtResponse
            {
                Id = c.Id,
                Name = c.Name,
                SportType = c.SportType,
                SlotDurationMinutes = c.SlotDurationMinutes,
                MinBookingSlots = c.MinBookingSlots,
                MaxBookingSlots = c.MaxBookingSlots,
                TimeZone = c.TimeZone
            }).ToList()
        };

        return Result.Ok(response);
    }
}