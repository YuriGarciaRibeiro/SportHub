using Application.Common.Interfaces;
using Application.CQRS;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Logging;

public class CreateCourtHandler : ICommandHandler<CreateCourtCommand>
{
    private readonly IEstablishmentsRepository _establishmentRepository;
    private readonly ICourtsRepository _courtsRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IEstablishmentRoleService _establishmentRoleService;
    private readonly ILogger<CreateCourtHandler> _logger;

    public CreateCourtHandler(
        IEstablishmentsRepository establishmentRepository,
        ICourtsRepository courtsRepository,
        ICurrentUserService currentUser,
        IEstablishmentRoleService establishmentRoleService,
        ILogger<CreateCourtHandler> logger)
    {
        _establishmentRepository = establishmentRepository;
        _courtsRepository = courtsRepository;
        _currentUser = currentUser;
        _establishmentRoleService = establishmentRoleService;
        _logger = logger;
    }

    public async Task<Result> Handle(CreateCourtCommand request, CancellationToken cancellationToken)
    {
        var establishmentResult = await _establishmentRepository.GetByIdAsync(request.EstablishmentId);
        if (establishmentResult == null)
        {
            return Result.Fail("Establishment not found.");
        }

        var isManager = await _establishmentRoleService.HasAtLeastRoleAsync(
            _currentUser.UserId, request.EstablishmentId, EstablishmentRole.Manager);

        if (!isManager)
        {
            return Result.Fail("You do not have permission to create a court in this establishment.");
        }

        _logger.LogInformation($"Creating court for establishment: {request.Court.Name} in {establishmentResult.Name}");

        var court = new Court
        {
            Name = request.Court.Name,
            EstablishmentId = request.EstablishmentId,
            MaxBookingSlots = request.Court.MaxBookingSlots,
            MinBookingSlots = request.Court.MinBookingSlots,
            SlotDurationMinutes = request.Court.SlotDurationMinutes,
            SportType = request.Court.SportType,
            TimeZone = request.Court.TimeZone,

        };  

        await _courtsRepository.CreateAsync(court);
        return Result.Ok();
    }
}
