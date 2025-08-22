using Application.Common.Enums;
using Application.Common.Errors;
using Application.Services;
using Domain.Entities;

public class ReservationService : BaseService<Reservation>, IReservationService
{
    private readonly IReservationRepository _reservationRepository;
    private readonly ICourtsRepository _courtRepository;

    public ReservationService(IReservationRepository reservationRepository, ICourtsRepository courtRepository, ICacheService cacheService)
        : base(reservationRepository, cacheService)
    {
        _reservationRepository = reservationRepository;
        _courtRepository = courtRepository;
    }

    public async Task<Result<IEnumerable<DateTime>>> GetAvailableSlotsAsync(Guid courtId, DateTime day, CancellationToken cancellationToken)
    {
        var key = _cache.GenerateCacheKey(CacheKeyPrefix.Query, nameof(Reservation), "availableSlots", courtId, day);
        var cached = await _cache.GetAsync<IEnumerable<DateTime>>(key, cancellationToken);
        if (cached != null) return Result.Ok(cached);

        var court = await _courtRepository.GetByIdAsync(courtId, cancellationToken);
        if (court == null) return Result.Fail(new NotFound("Court not found"));

        var startHour = new TimeSpan(court.OpeningTime.Hour, court.OpeningTime.Minute, court.OpeningTime.Second);
        var endHour = new TimeSpan(court.ClosingTime.Hour, court.ClosingTime.Minute, court.ClosingTime.Second);

        var slotDuration = TimeSpan.FromMinutes(court.SlotDurationMinutes);

        var existingReservations = await _reservationRepository
            .GetByCourtAndDayAsync(courtId, day, cancellationToken);

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

    public Task<Guid?> GetEstablishmentIdByReservationAsync(Guid reservationId, CancellationToken cancellationToken)
    {
        return _reservationRepository.GetEstablishmentIdByReservationAsync(reservationId, cancellationToken);
    }

    public Task<bool> IsReservationOwnerAsync(Guid reservationId, Guid userId, CancellationToken cancellationToken)
    {
        return _reservationRepository.IsReservationOwnerAsync(reservationId, userId, cancellationToken);
    }

    public async Task<Result<Guid>> ReserveAsync(Court court, Guid userId, DateTime startUtc, DateTime endUtc, CancellationToken cancellationToken)
    {
        var opening = DateTime.SpecifyKind(startUtc.Date + court.OpeningTime.ToTimeSpan(), DateTimeKind.Utc);
        var closing = DateTime.SpecifyKind(startUtc.Date + court.ClosingTime.ToTimeSpan(), DateTimeKind.Utc);

        if (startUtc < opening || endUtc > closing)
            return Result.Fail("Reservation is outside of court operating hours");

        var totalDuration = endUtc - startUtc;
        var slotDuration = TimeSpan.FromMinutes(court.SlotDurationMinutes);

        if (totalDuration.TotalMinutes % slotDuration.TotalMinutes != 0) return Result.Fail("Reservation duration must be a multiple of slot duration");

        var totalSlots = totalDuration.TotalMinutes / slotDuration.TotalMinutes;
        if (totalSlots > court.MaxBookingSlots) return Result.Fail($"Reservation exceeds the maximum of {court.MaxBookingSlots} slots allowed");
        if (totalSlots < court.MinBookingSlots) return Result.Fail($"Reservation must be at least {court.MinBookingSlots} slots");

        
        var hasConflict = await _reservationRepository.ExistsConflictAsync(court.Id, startUtc, endUtc, cancellationToken);
        if (hasConflict)
            return Result.Fail("Time slot is already booked");

        var reservation = new Reservation
        {
            CourtId = court.Id,
            UserId = userId,
            StartTimeUtc = startUtc,
            EndTimeUtc = endUtc
        };

        await _reservationRepository.AddAsync(reservation, cancellationToken);

        var day = startUtc.Date;
        var key = _cache.GenerateCacheKey(CacheKeyPrefix.Query, nameof(Reservation), "availableSlots", court.Id, day);
        await _cache.RemoveAsync(key, cancellationToken);

        return Result.Ok(reservation.Id);
    }
}
