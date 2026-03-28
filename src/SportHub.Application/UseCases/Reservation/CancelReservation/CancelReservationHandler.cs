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
            IncludeCourt = false,
            IncludeTenant = true,
            IncludeUser = false,
            AsNoTracking = true
        });

        if (reservation is null)
            return Result.Fail(new NotFound($"Reserva com ID {request.ReservationId} não encontrada."));

        var currentUserId = _currentUserService.UserId;
        var currentRole = _currentUserService.UserRole;
        var isManagerOrAbove = currentRole >= UserRole.Manager;

        if (reservation.UserId != currentUserId && !isManagerOrAbove)
            return Result.Fail(new Forbidden("Você não tem permissão para cancelar esta reserva."));
        
        if (DateTime.UtcNow + TimeSpan.FromHours(reservation.Tenant.CancelationWindowHours) > reservation.StartTimeUtc)
            return Result.Fail(new BadRequest($"Reservas só podem ser canceladas com pelo menos {reservation.Tenant.CancelationWindowHours} horas de antecedência."));

        _logger.LogInformation("Cancelando reserva {ReservationId} por usuário {UserId}", request.ReservationId, currentUserId);

        await _reservationRepository.RemoveAsync(reservation);

        var cacheKey = _cacheService.GenerateCacheKey(CacheKeyPrefix.GetAvailability, reservation.CourtId, reservation.StartTimeUtc.ToString("yyyy-MM-dd"));
        await _cacheService.RemoveAsync(cacheKey, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
