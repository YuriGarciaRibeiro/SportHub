using Application.Common.Interfaces;
using Application.CQRS;

public class DeleteCourtHandler : ICommandHandler<DeleteCourtCommand>
{
    private readonly ICourtsRepository _courtRepository;
    private readonly IReservationRepository _reservationRepository;

    public DeleteCourtHandler(ICourtsRepository courtRepository, IReservationRepository reservationRepository)
    {
        _courtRepository = courtRepository;
        _reservationRepository = reservationRepository;
    }

    public async Task<Result> Handle(DeleteCourtCommand command, CancellationToken cancellationToken)
    {
        var court = await _courtRepository.GetByIdAsync(command.Id);
        if (court == null)
        {
            return Result.Fail("Court not found.");
        }

        var reservations = await _reservationRepository.GetFutureReservationsByCourtAsync(court.Id);
        if (reservations.Any())
        {
            return Result.Fail("Cannot delete court with existing reservations.");
        }

        await _courtRepository.RemoveAsync(court);
        return Result.Ok();
    }
}