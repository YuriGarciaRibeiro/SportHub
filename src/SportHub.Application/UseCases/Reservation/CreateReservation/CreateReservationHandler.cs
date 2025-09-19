using Application.Common.Errors;
using Application.Common.Interfaces.Email;
using Application.Emails.reservation_confirmed;
using Microsoft.Extensions.Logging;
using SportHub.Application.Common.Interfaces.Email;

namespace Application.UseCases.Reservations.CreateReservation;

public class CreateReservationHandler : ICommandHandler<CreateReservationCommand, CreateReservationResponse>
{
    private readonly IReservationService _reservationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICourtService _courtService;
    private readonly ICustomEmailSender _emailService;
    private readonly IEmailTemplateService _emailTemplateService;
    private readonly ILogger<CreateReservationHandler> _logger;

    public CreateReservationHandler(IReservationService reservationService, ICurrentUserService currentUserService, ICourtService courtService, ICustomEmailSender emailService, IEmailTemplateService emailTemplateService, ILogger<CreateReservationHandler> logger)
    {
        _logger = logger;
        _courtService = courtService;
        _currentUserService = currentUserService;
        _reservationService = reservationService;
        _emailService = emailService;
        _emailTemplateService = emailTemplateService;
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

        // Send confirmation email
        var emailModel = new ReservationConfirmedEmailModel(
            establishmentName: court.Establishment.Name,
            startDateTime: request.Reservation.StartTime.ToString("f"),
            endDateTime: request.Reservation.EndTime.ToString("f"),
            userName: _currentUserService.FullName,
            reservationId: reservationResult.Value.ToString(),
            courtName: court.Name,
            duration: (request.Reservation.EndTime - request.Reservation.StartTime).TotalHours + " hours",
            price: (court.PricePerSlot * (decimal)(request.Reservation.EndTime - request.Reservation.StartTime).TotalHours).ToString("C"),
            supportUrl: "https://support.sporthub.com",
            year: DateTime.UtcNow.Year.ToString()
        );

        var emailContent = await _emailTemplateService.RenderTemplateAsync("ReservationConfirmed", emailModel, cancellationToken);
        await _emailService.SendAsync(
            to: _currentUserService.Email,
            subject: "Your Reservation is Confirmed!",
            body: emailContent
        );

        return Result.Ok(new CreateReservationResponse
        {
            ReservationId = reservationResult.Value
        });
    }
}
    
