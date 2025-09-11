using Application.Common.Errors;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.Court.GetCourtsByEstablishmentId;

public class GetCourtsByEstablishmentIdQueryHandler : IQueryHandler<GetCourtsByEstablishmentIdQuery, GetCourtsByEstablishmentIdResponse>
{
    private readonly ICourtService _courtService;
    private readonly IEstablishmentService _establishmentService;
    private readonly ILogger<GetCourtsByEstablishmentIdQueryHandler> _logger;

    public GetCourtsByEstablishmentIdQueryHandler(ICourtService courtService, ILogger<GetCourtsByEstablishmentIdQueryHandler> logger, IEstablishmentService establishmentService)
    {
        _courtService = courtService;
        _logger = logger;
        _establishmentService = establishmentService;
    }

    public async Task<Result<GetCourtsByEstablishmentIdResponse>> Handle(GetCourtsByEstablishmentIdQuery request, CancellationToken cancellationToken)
    {

        var establishment = await _establishmentService.GetByIdAsync(request.EstablishmentId, ct: cancellationToken);
        if (establishment == null)
        {
            _logger.LogWarning("Establishment with ID {EstablishmentId} not found.", request.EstablishmentId);
            return Result.Fail(new NotFound($"Establishment with ID {request.EstablishmentId} not found."));
        }

        var courts = await _courtService.GetCourtsByEstablishmentIdAsync(request.EstablishmentId, cancellationToken);

        var response = new GetCourtsByEstablishmentIdResponse
        {
            EstablishmentId = request.EstablishmentId,
            Courts = courts.Select(c => new CourtResponse
            {
                Id = c.Id,
                Name = c.Name,
                SlotDurationMinutes = c.SlotDurationMinutes,
                MinBookingSlots = c.MinBookingSlots,
                MaxBookingSlots = c.MaxBookingSlots,
                Sports = c.Sports.Select(s => new SportResponse
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description
                })
            })
        };

        return Result.Ok(response);
    }
}