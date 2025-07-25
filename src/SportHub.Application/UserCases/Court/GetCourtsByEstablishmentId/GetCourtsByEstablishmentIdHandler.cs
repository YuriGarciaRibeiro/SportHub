using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.CQRS;
using Microsoft.Extensions.Logging;

namespace Application.UserCases.Court.GetCourtsByEstablishmentId;

public class GetCourtsByEstablishmentIdQueryHandler : IQueryHandler<GetCourtsByEstablishmentIdQuery, GetCourtsByEstablishmentIdResponse>
{
    private readonly ICourtsRepository _courtsRepository;
    private readonly IEstablishmentService _establishmentService;
    private readonly ILogger<GetCourtsByEstablishmentIdQueryHandler> _logger;

    public GetCourtsByEstablishmentIdQueryHandler(ICourtsRepository courtsRepository, ILogger<GetCourtsByEstablishmentIdQueryHandler> logger, IEstablishmentService establishmentService)
    {
        _courtsRepository = courtsRepository;
        _logger = logger;
        _establishmentService = establishmentService;
    }

    public async Task<Result<GetCourtsByEstablishmentIdResponse>> Handle(GetCourtsByEstablishmentIdQuery request, CancellationToken cancellationToken)
    {

        var establishment = await _establishmentService.GetEstablishmentByIdAsync(request.EstablishmentId);
        if (establishment == null)
        {
            _logger.LogWarning("Establishment with ID {EstablishmentId} not found.", request.EstablishmentId);
            return Result.Fail(new NotFound($"Establishment with ID {request.EstablishmentId} not found."));
        }


        var courts = await _courtsRepository.GetByEstablishmentIdAsync(request.EstablishmentId);

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