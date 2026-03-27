using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.CQRS;
using Application.UseCases.Court.GetCourtById;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.Court.UpdateCourt;

public class UpdateCourtHandler : ICommandHandler<UpdateCourtCommand, CourtPublicResponse>
{
    private readonly ICourtsRepository _courtsRepository;
    private readonly ISportsRepository _sportsRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateCourtHandler> _logger;

    public UpdateCourtHandler(
        ICourtsRepository courtsRepository,
        ISportsRepository sportsRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateCourtHandler> logger)
    {
        _courtsRepository = courtsRepository;
        _sportsRepository = sportsRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<CourtPublicResponse>> Handle(UpdateCourtCommand request, CancellationToken cancellationToken)
    {
        var court = await _courtsRepository.GetByIdAsync(request.Id, new GetCourtIncludeSettings { IncludeSports = true, AsNoTracking = false });

        if (court is null)
            return Result.Fail(new NotFound($"Quadra com ID {request.Id} não encontrada."));

        var sports = await _sportsRepository.GetSportsByIdsAsync(request.Court.Sports);

        _logger.LogInformation("Atualizando quadra: {CourtId} - {CourtName}", request.Id, request.Court.Name);

        court.Name = request.Court.Name;
        court.ImageUrl = request.Court.ImageUrl;
        court.PricePerHour = request.Court.PricePerHour;
        court.SlotDurationMinutes = request.Court.SlotDurationMinutes;
        court.MinBookingSlots = request.Court.MinBookingSlots;
        court.MaxBookingSlots = request.Court.MaxBookingSlots;
        court.OpeningTime = request.Court.OpeningTime;
        court.ClosingTime = request.Court.ClosingTime;
        court.TimeZone = request.Court.TimeZone;
        court.Amenities = request.Court.Amenities;
        court.LocationId = request.Court.LocationId;
        court.Sports.Clear();
        foreach (var sport in sports)
            court.Sports.Add(sport);
        court.PeakPricePerHour = request.Court.PeakPricePerHour;


        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new CourtPublicResponse(
            court.Id,
            court.Name,
            court.ImageUrl,
            court.ImageUrls,
            court.PricePerHour,
            court.SlotDurationMinutes,
            court.MinBookingSlots,
            court.MaxBookingSlots,
            court.OpeningTime.ToString("HH:mm"),
            court.ClosingTime.ToString("HH:mm"),
            court.Amenities,
            court.Sports.Select(s => new SportSummary(s.Id, s.Name)).ToList(),
            court.LocationId,
            court.Location?.Name,
            court.PeakPricePerHour,
            court.PeakStartTime,
            court.PeakEndTime
        );

        return Result.Ok(response);
    }
}
