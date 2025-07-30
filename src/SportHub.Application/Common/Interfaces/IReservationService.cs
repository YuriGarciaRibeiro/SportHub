namespace Application.Common.Interfaces;

public interface IReservationService
{
    Task<Result<List<DateTime>>> GetAvailableSlotsAsync(Guid courtId, DateTime day);
    Task<Result> ReserveAsync(Guid courtId, Guid userId, DateTime startUtc);
}
