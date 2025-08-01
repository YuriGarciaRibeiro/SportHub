using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IReservationService
{
    Task<Result<List<DateTime>>> GetAvailableSlotsAsync(Guid courtId, DateTime day);
    Task<Result<Guid>> ReserveAsync(Court court, Guid userId, DateTime startUtc, DateTime endUtc);
}
