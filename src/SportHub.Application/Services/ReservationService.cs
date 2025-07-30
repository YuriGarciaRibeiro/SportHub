using Application.Common.Errors;
using Application.Common.Interfaces;
using Domain.Entities;

public class ReservationService : IReservationService
{
    private readonly IReservationRepository _reservationRepository;
    private readonly ICourtsRepository _courtRepository;

    public ReservationService(IReservationRepository reservationRepository, ICourtsRepository courtRepository)
    {
        _reservationRepository = reservationRepository;
        _courtRepository = courtRepository;
    }

    public async Task<Result<List<DateTime>>> GetAvailableSlotsAsync(Guid courtId, DateTime day)
    {
        var court = await _courtRepository.GetByIdAsync(courtId);
        if (court == null) return Result.Fail(new NotFound("Court not found"));

        var startHour = new TimeSpan(court.OpeningTime.Hour, court.OpeningTime.Minute, court.OpeningTime.Second);
        var endHour = new TimeSpan(court.ClosingTime.Hour, court.ClosingTime.Minute, court.ClosingTime.Second);

        var slotDuration = TimeSpan.FromMinutes(court.SlotDurationMinutes);

        var existingReservations = await _reservationRepository
            .GetByCourtAndDayAsync(courtId, day);

        var slots = new List<DateTime>();
        for (var time = startHour; time + slotDuration <= endHour; time += slotDuration)
        {
            var slotStart = day.Date + time;
            var slotEnd = slotStart + slotDuration;

            bool conflict = existingReservations.Any(r =>
                slotStart < r.EndTimeUtc && slotEnd > r.StartTimeUtc);

            if (!conflict)
                slots.Add(slotStart);
        }

        return slots;
    }


    public async Task<Result> ReserveAsync(Guid courtId, Guid userId, DateTime startUtc)
    {
        var court = await _courtRepository.GetByIdAsync(courtId);
        if (court == null) return Result.Fail("Court not found");

        var duration = TimeSpan.FromMinutes(court.SlotDurationMinutes);
        var endUtc = startUtc + duration;

        var hasConflict = await _reservationRepository
            .ExistsConflictAsync(courtId, startUtc, endUtc);

        if (hasConflict)
            return Result.Fail("Time slot is already booked");

        var reservation = new Reservation
        {
            Id = Guid.NewGuid(),
            CourtId = courtId,
            UserId = userId,
            StartTimeUtc = startUtc,
            EndTimeUtc = endUtc
        };

        await _reservationRepository.AddAsync(reservation);
        return Result.Ok();
    }
}
