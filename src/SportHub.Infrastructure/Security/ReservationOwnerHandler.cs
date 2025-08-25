using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Application.Security;
using Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Security;

public class ReservationOwnerHandler : AuthorizationHandler<ReservationOwnerRequirement, HttpContext>
{
    private readonly IReservationService _reservationService;
    private readonly IEstablishmentRoleService _establishmentRoleService;
    private readonly IUserService _userService;
    private readonly ILogger<ReservationOwnerHandler> _logger;

    public ReservationOwnerHandler(
        IReservationService reservationService,
        IEstablishmentRoleService establishmentRoleService,
        IUserService userService,
        ILogger<ReservationOwnerHandler> logger)
    {
        _reservationService = reservationService;
        _establishmentRoleService = establishmentRoleService;
        _userService = userService;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ReservationOwnerRequirement requirement,
        HttpContext httpContext)
    {
        _logger.LogInformation($"Checking reservation access for route: {httpContext.Request.Path}");

        // Extrair reservationId da rota
        var reservationIdRaw = httpContext.GetRouteValue("reservationId");
        if (reservationIdRaw is null || !Guid.TryParse(reservationIdRaw.ToString(), out var reservationId))
        {
            _logger.LogWarning("Reservation ID not provided or invalid.");
            return;
        }

        // Extrair userId do token
        var subClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
        if (subClaim is null || !Guid.TryParse(subClaim.Value, out var userId))
        {
            _logger.LogWarning("User ID not found in claims.");
            return;
        }

        // 1. Verificar se o usuário é o dono direto da reserva
        var isDirectOwner = await _reservationService.IsReservationOwnerAsync(reservationId, userId, CancellationToken.None);
        if (isDirectOwner)
        {
            _logger.LogInformation($"User {userId} is direct owner of reservation {reservationId}");
            context.Succeed(requirement);
            return;
        }

        // 2. Verificar se é um Admin global
        var user = await _userService.GetUserByIdAsync(userId, CancellationToken.None);
        if (user != null && user.Value.Role == UserRole.Admin)
        {
            _logger.LogInformation($"User {userId} is global admin - allowing access to reservation {reservationId}");
            context.Succeed(requirement);
            return;
        }

        // 3. Verificar se é dono/manager do estabelecimento da reserva
        var establishmentId = await _reservationService.GetEstablishmentIdByReservationAsync(reservationId, CancellationToken.None);
        if (establishmentId.HasValue)
        {
            // Verificar se tem pelo menos role de Manager no estabelecimento
            var hasEstablishmentRole = await _establishmentRoleService.HasAtLeastRoleAsync(
                userId, 
                establishmentId.Value, 
                EstablishmentRole.Manager, 
                CancellationToken.None);

            if (hasEstablishmentRole)
            {
                _logger.LogInformation($"User {userId} is manager/owner of establishment {establishmentId.Value} - allowing access to reservation {reservationId}");
                context.Succeed(requirement);
                return;
            }
        }

        _logger.LogWarning($"User {userId} does not have permission to access reservation {reservationId}");
    }
}