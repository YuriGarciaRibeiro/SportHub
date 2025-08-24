using Application.Common.Interfaces;
using Application.CQRS;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.Court.CreateCourt;

public class CreateCourtHandler : ICommandHandler<CreateCourtCommand, GetCourtResponse>
{
    private readonly IEstablishmentService _establishmentService;
    private readonly ICourtService _courtService;
    private readonly ICurrentUserService _currentUser;
    private readonly IEstablishmentRoleService _establishmentRoleService;
    private readonly ILogger<CreateCourtHandler> _logger;
    private readonly ISportService _sportService;

    public CreateCourtHandler(
        IEstablishmentService establishmentService,
        ICourtService courtService,
        ICurrentUserService currentUser,
        IEstablishmentRoleService establishmentRoleService,
        ISportService sportService,
        ILogger<CreateCourtHandler> logger)
    {
        _establishmentService = establishmentService;
        _courtService = courtService;
        _currentUser = currentUser;
        _establishmentRoleService = establishmentRoleService;
        _sportService = sportService;
        _logger = logger;
    }

    public async Task<Result<GetCourtResponse>> Handle(CreateCourtCommand request, CancellationToken cancellationToken)
    {
        var establishmentResult = await _establishmentService.GetByIdNoTrackingAsync(request.EstablishmentId, ct: cancellationToken);
        if (establishmentResult == null)
        {
            return Result.Fail("Establishment not found.");
        }

        var sports = await _sportService.GetByIdsAsync(request.Court.Sports, cancellationToken);

        _logger.LogInformation($"Creating court for establishment: {request.Court.Name} in {establishmentResult.Name}");
        var court = new Domain.Entities.Court
        {
            Name = request.Court.Name,
            EstablishmentId = request.EstablishmentId,
            MaxBookingSlots = request.Court.MaxBookingSlots,
            MinBookingSlots = request.Court.MinBookingSlots,
            SlotDurationMinutes = request.Court.SlotDurationMinutes,
            OpeningTime = request.Court.OpeningTime,
            ClosingTime = request.Court.ClosingTime,
            TimeZone = request.Court.TimeZone,
            Sports = sports.ToList()
        };

        await _courtService.CreateAsync(court, cancellationToken);

        var courtResponse = new GetCourtResponse
        {
            Id = court.Id,
            Name = court.Name,
            MaxBookingSlots = court.MaxBookingSlots,
            MinBookingSlots = court.MinBookingSlots,
            SlotDurationMinutes = court.SlotDurationMinutes,
            OpeningTime = court.OpeningTime,
            ClosingTime = court.ClosingTime,
            TimeZone = court.TimeZone,
            Sports = court.Sports.Select(s => new SportResponse
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description
            }).ToList()
        };
        return Result.Ok(courtResponse);
    }
}
