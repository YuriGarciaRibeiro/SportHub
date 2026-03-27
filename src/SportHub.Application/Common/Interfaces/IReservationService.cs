using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IReservationService
{
    Task<Result<List<(DateTime SlotUtc, bool IsAvailable)>>> GetSlotsAsync(Guid courtId, DateTime day);
    Task<Result<Reservation>> ReserveAsync(Court court, Guid userId, DateTime startUtc, DateTime endUtc, bool peakHoursEnabled);
}
