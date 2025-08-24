using Application.Common.Interfaces;
using Application.CQRS;

public class DeleteCourtHandler : ICommandHandler<DeleteCourtCommand>
{
    private readonly ICourtService _courtService;
    private readonly IReservationService _reservationService;

    public DeleteCourtHandler(ICourtService courtService, IReservationService reservationService)
    {
        _courtService = courtService;
        _reservationService = reservationService;
    }

    public async Task<Result> Handle(DeleteCourtCommand command, CancellationToken cancellationToken)
    {
        var court = await _courtService.GetByIdNoTrackingAsync(command.Id, ct: cancellationToken);
        if (court == null)
        {
            return Result.Fail("Court not found.");
        }

        var reservations = await _reservationService.GetFutureReservationsByCourtAsync(court.Id, cancellationToken);
        if (reservations.Any())
        {
            return Result.Fail("Cannot delete court with existing reservations.");
        }

        await _courtService.DeleteAsync(court, cancellationToken);
        return Result.Ok();
    }
}