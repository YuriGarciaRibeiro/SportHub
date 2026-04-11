using Application.Common.Enums;
using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.CQRS;
using Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.Reservations.CancelReservation;

public class CancelReservationHandler : ICommandHandler<CancelReservationCommand>
{
    private readonly IReservationRepository _reservationRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CancelReservationHandler> _logger;

    public CancelReservationHandler(
        IReservationRepository reservationRepository,
        ICurrentUserService currentUserService,
        ICacheService cacheService,
        IUnitOfWork unitOfWork,
        ILogger<CancelReservationHandler> logger)
    {
        _reservationRepository = reservationRepository;
        _currentUserService = currentUserService;
        _cacheService = cacheService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(CancelReservationCommand request, CancellationToken cancellationToken)
    {
        var reservation = await _reservationRepository.GetByIdAsync(request.ReservationId, new GetReservationSettings
        {
            IncludeCourt = true,
            IncludeTenant = true,
            IncludeUser = false,
            AsNoTracking = false
        });

        if (reservation is null)
            return Result.Fail(new NotFound($"Reserva com ID {request.ReservationId} não encontrada."));

        var currentUserId = _currentUserService.UserId;
        var currentRole = _currentUserService.UserRole;
        var isManagerOrAbove = currentRole >= UserRole.Manager;

        if (reservation.UserId != currentUserId && !isManagerOrAbove)
            return Result.Fail(new Forbidden("Você não tem permissão para cancelar esta reserva."));
        
        var effectiveWindowHours = reservation.Court.CancelationWindowHours
            ?? reservation.Tenant.CancelationWindowHours;

        var isWithinWindow = DateTime.UtcNow + TimeSpan.FromHours(effectiveWindowHours) > reservation.StartTimeUtc;

        if (isWithinWindow)
        {
            if (reservation.Court.LateCancellationFeePercent is > 0)
            {
                var feeAmount = reservation.TotalPrice * (reservation.Court.LateCancellationFeePercent.Value / 100m);
                return Result.Fail(new BadRequest(
                    $"Esta reserva está dentro da janela de cancelamento de {effectiveWindowHours} hora(s). " +
                    $"Uma taxa de cancelamento tardio de {reservation.Court.LateCancellationFeePercent}% " +
                    $"(R$ {feeAmount:F2}) seria aplicada. Cancelamento não permitido."));
            }

            return Result.Fail(new BadRequest(
                $"Reservas só podem ser canceladas com pelo menos {effectiveWindowHours} horas de antecedência."));
        }

        _logger.LogInformation("Cancelando reserva {ReservationId} por usuário {UserId}", request.ReservationId, currentUserId);

        reservation.Status = ReservationStatus.Cancelled;
        await _reservationRepository.UpdateAsync(reservation);

        var cacheKey = _cacheService.GenerateCacheKey(CacheKeyPrefix.GetAvailability, reservation.CourtId, reservation.StartTimeUtc.ToString("yyyy-MM-dd"));
        await _cacheService.RemoveAsync(cacheKey, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
