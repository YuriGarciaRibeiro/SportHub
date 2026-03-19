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
    private readonly ICourtsRepository _courtsRepository;
    private readonly ICacheService _cacheService;
    private readonly IRealtimeNotificationService _notificationService;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<CreateReservationHandler> _logger;

    public CreateReservationHandler(IReservationService reservationService, ICurrentUserService currentUserService, ICourtsRepository courtsRepository, ICacheService cacheService, IRealtimeNotificationService notificationService, ITenantContext tenantContext, ILogger<CreateReservationHandler> logger)
    {
        _logger = logger;
        _courtsRepository = courtsRepository;
        _currentUserService = currentUserService;
        _reservationService = reservationService;
        _cacheService = cacheService;
        _notificationService = notificationService;
        _tenantContext = tenantContext;
    }

    public async Task<Result<CreateReservationResponse>> Handle(CreateReservationCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;

        // If a TargetUserId is provided (staff booking on behalf of a customer), use it as the reservation owner.
        // Only staff members (role != null) are allowed to specify a different target user.
        var targetUserId = request.Reservation.TargetUserId;
        if (targetUserId.HasValue && _currentUserService.UserRole is null or Domain.Enums.UserRole.Customer)
            return Result.Fail(new Forbidden("Only staff members can create reservations on behalf of others."));

        var userId = targetUserId ?? currentUserId;

        var court = await _courtsRepository.GetByIdAsync(request.CourtId);
        if (court == null)
        {
            _logger.LogWarning($"Court with ID {request.CourtId} not found.");
            return Result.Fail(new NotFound($"Court with ID {request.CourtId} not found."));
        }

        var reservationResult = await _reservationService.ReserveAsync(court, userId, request.Reservation.StartTime.ToUniversalTime(), request.Reservation.EndTime.ToUniversalTime());

        if (reservationResult.IsFailed)
        {
            _logger.LogWarning($"Failed to create reservation for Court ID {request.CourtId} by User ID {userId}: {reservationResult.Errors}");
            return Result.Fail(reservationResult.Errors);
        }

        _logger.LogInformation($"Reservation created successfully for Court ID {request.CourtId} by User ID {userId}.");

        var cacheKey = _cacheService.GenerateCacheKey(CacheKeyPrefix.GetAvailability, request.CourtId, request.Reservation.StartTime.ToString("yyyy-MM-dd"));
        await _cacheService.RemoveAsync(cacheKey, cancellationToken);

        var payload = new ReservationCreatedPayload(
            ReservationId: reservationResult.Value,
            CourtId: request.CourtId,
            UserId: userId,
            StartTime: request.Reservation.StartTime.ToUniversalTime(),
            EndTime: request.Reservation.EndTime.ToUniversalTime()
        );
        await _notificationService.NotifyReservationCreatedAsync(_tenantContext.Schema, payload, cancellationToken);

        return Result.Ok(new CreateReservationResponse
        {
            ReservationId = reservationResult.Value
        });
    }
}
    
