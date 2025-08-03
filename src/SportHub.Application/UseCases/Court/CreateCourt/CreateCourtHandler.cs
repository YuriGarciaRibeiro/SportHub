using Application.Common.Interfaces;
using Application.CQRS;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.Court.CreateCourt;

public class CreateCourtHandler : ICommandHandler<CreateCourtCommand, GetCourtResponse>
{
    private readonly IEstablishmentsRepository _establishmentRepository;
    private readonly ICourtsRepository _courtsRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IEstablishmentRoleService _establishmentRoleService;
    private readonly ILogger<CreateCourtHandler> _logger;
    private readonly ISportsRepository _sportsRepository;

    public CreateCourtHandler(
        IEstablishmentsRepository establishmentRepository,
        ICourtsRepository courtsRepository,
        ICurrentUserService currentUser,
        IEstablishmentRoleService establishmentRoleService,
        ISportsRepository sportsRepository,
        ILogger<CreateCourtHandler> logger)
    {
        _establishmentRepository = establishmentRepository;
        _courtsRepository = courtsRepository;
        _currentUser = currentUser;
        _establishmentRoleService = establishmentRoleService;
        _sportsRepository = sportsRepository;
        _logger = logger;
    }

    public async Task<Result<GetCourtResponse>> Handle(CreateCourtCommand request, CancellationToken cancellationToken)
    {
        var establishmentResult = await _establishmentRepository.GetByIdAsync(request.EstablishmentId);
        if (establishmentResult == null)
        {
            return Result.Fail("Establishment not found.");
        }

        var sports = await _sportsRepository.GetSportsByIdsAsync(request.Court.Sports);

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

        await _courtsRepository.AddAsync(court);

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
