using Application.Common.Interfaces;
using Application.CQRS;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.Court.CreateCourt;

public class CreateCourtHandler : ICommandHandler<CreateCourtCommand, Guid>
{
    private readonly ICourtsRepository _courtsRepository;
    private readonly ISportsRepository _sportsRepository;
    private readonly ILogger<CreateCourtHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCourtHandler(
        ICourtsRepository courtsRepository,
        ISportsRepository sportsRepository,
        ILogger<CreateCourtHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _courtsRepository = courtsRepository;
        _sportsRepository = sportsRepository;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateCourtCommand request, CancellationToken cancellationToken)
    {
        var sports = await _sportsRepository.GetSportsByIdsAsync(request.Court.Sports);

        _logger.LogInformation("Creating court: {CourtName}", request.Court.Name);

        var court = new Domain.Entities.Court
        {
            Name = request.Court.Name,
            ImageUrl = request.Court.ImageUrl,
            PricePerHour = request.Court.PricePerHour,
            MaxBookingSlots = request.Court.MaxBookingSlots,
            MinBookingSlots = request.Court.MinBookingSlots,
            SlotDurationMinutes = request.Court.SlotDurationMinutes,
            OpeningTime = request.Court.OpeningTime,
            ClosingTime = request.Court.ClosingTime,
            TimeZone = request.Court.TimeZone,
            Amenities = request.Court.Amenities,
            LocationId = request.Court.LocationId,
            Sports = sports.ToList(),
            PeakPricePerHour = request.Court.PeakPricePerHour,
            CancelationWindowHours = request.Court.CancelationWindowHours,
            LateCancellationFeePercent = request.Court.LateCancellationFeePercent
        };

        await _courtsRepository.AddAsync(court);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Ok(court.Id);
    }
}
