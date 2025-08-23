using Application.Common.Enums;
using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.CQRS;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.Reservations.CreateReservation;

public class CreateReservationHandler : ICommandHandler<CreateReservationCommand, CreateReservationResponse>
{
    private readonly IReservationService _reservationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICourtService _courtService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<CreateReservationHandler> _logger;

    public CreateReservationHandler(IReservationService reservationService, ICurrentUserService currentUserService, ICourtService courtService, ICacheService cacheService, ILogger<CreateReservationHandler> logger)
    {
        _logger = logger;
        _courtService = courtService;
        _currentUserService = currentUserService;
        _reservationService = reservationService;
        _cacheService = cacheService;
    }

    public async Task<Result<CreateReservationResponse>> Handle(CreateReservationCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        
        var court = await _courtService.GetByIdAsync(request.CourtId, ct: cancellationToken);
        if (court == null)
        {
            _logger.LogWarning($"Court with ID {request.CourtId} not found.");
            return Result.Fail(new NotFound($"Court with ID {request.CourtId} not found."));
        }

        var reservationResult = await _reservationService.ReserveAsync(
            court,
            userId,
            request.Reservation.StartTime.ToUniversalTime(),
            request.Reservation.EndTime.ToUniversalTime(),
            cancellationToken);

        if (reservationResult.IsFailed)
        {
            _logger.LogWarning($"Failed to create reservation for Court ID {request.CourtId} by User ID {userId}: {reservationResult.Errors}");
            return Result.Fail(reservationResult.Errors);
        }

        _logger.LogInformation($"Reservation created successfully for Court ID {request.CourtId} by User ID {userId}.");

        return Result.Ok(new CreateReservationResponse
        {
            ReservationId = reservationResult.Value
        });
    }
}
    
