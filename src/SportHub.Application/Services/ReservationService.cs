using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.Services;
using Domain.Entities;

public class ReservationService : IReservationService
{
    private readonly IReservationRepository _reservationRepository;
    private readonly ICourtsRepository _courtRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ReservationService(IReservationRepository reservationRepository, ICourtsRepository courtRepository, IUnitOfWork unitOfWork)
    {
        _reservationRepository = reservationRepository;
        _courtRepository = courtRepository;
        _unitOfWork = unitOfWork;
    }

    private static readonly TimeZoneInfo BrazilTz = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    public async Task<Result<List<(DateTime SlotUtc, bool IsAvailable)>>> GetSlotsAsync(Guid courtId, DateTime day)
    {
        var court = await _courtRepository.GetByIdAsync(courtId);
        if (court == null) return Result.Fail(new NotFound("Court not found"));

        var localDate = day.Date;
        var openingLocal = DateTime.SpecifyKind(localDate + court.OpeningTime.ToTimeSpan(), DateTimeKind.Unspecified);
        var closingLocal = DateTime.SpecifyKind(localDate + court.ClosingTime.ToTimeSpan(), DateTimeKind.Unspecified);
        var openingUtc = TimeZoneInfo.ConvertTimeToUtc(openingLocal, BrazilTz);
        var closingUtc = TimeZoneInfo.ConvertTimeToUtc(closingLocal, BrazilTz);

        var slotDuration = TimeSpan.FromMinutes(court.SlotDurationMinutes);
        var existingReservations = await _reservationRepository.GetByCourtAndDayAsync(courtId, day);

        var slots = new List<(DateTime, bool)>();
        for (var slotUtc = openingUtc; slotUtc + slotDuration <= closingUtc; slotUtc += slotDuration)
        {
            var slotEnd = slotUtc + slotDuration;
            var isAvailable = !existingReservations.Any(r => slotUtc < r.EndTimeUtc && slotEnd > r.StartTimeUtc);
            slots.Add((slotUtc, isAvailable));
        }

        return slots;
    }


    public async Task<Result<Reservation>> ReserveAsync(Court court, Guid userId, DateTime startUtc, DateTime endUtc, bool peakHoursEnabled)
    {
        var localDate = TimeZoneInfo.ConvertTimeFromUtc(startUtc, BrazilTz).Date;
        var openingLocal = DateTime.SpecifyKind(localDate + court.OpeningTime.ToTimeSpan(), DateTimeKind.Unspecified);
        var closingLocal = DateTime.SpecifyKind(localDate + court.ClosingTime.ToTimeSpan(), DateTimeKind.Unspecified);
        var opening = TimeZoneInfo.ConvertTimeToUtc(openingLocal, BrazilTz);
        var closing = TimeZoneInfo.ConvertTimeToUtc(closingLocal, BrazilTz);

        if (startUtc < opening || endUtc > closing)
            return Result.Fail("Reservation is outside of court operating hours");

        var totalDuration = endUtc - startUtc;
        var slotDuration = TimeSpan.FromMinutes(court.SlotDurationMinutes);

        if (totalDuration.TotalMinutes % slotDuration.TotalMinutes != 0) return Result.Fail("Reservation duration must be a multiple of slot duration");

        var totalSlots = totalDuration.TotalMinutes / slotDuration.TotalMinutes;
        if (totalSlots > court.MaxBookingSlots) return Result.Fail($"Reservation exceeds the maximum of {court.MaxBookingSlots} slots allowed");
        if (totalSlots < court.MinBookingSlots) return Result.Fail($"Reservation must be at least {court.MinBookingSlots} slots");

        
        var hasConflict = await _reservationRepository.ExistsConflictAsync(court.Id, startUtc, endUtc);
        if (hasConflict)
            return Result.Fail("Time slot is already booked");
        
        var pricing = PricingCalculator.Calculate(court, peakHoursEnabled, startUtc, endUtc);

        Reservation reservation = new Reservation
        {
            CourtId = court.Id,
            UserId = userId,
            StartTimeUtc = startUtc,
            EndTimeUtc = endUtc,
            IsPeakHours = pricing.IsPeakHours,
            PricePerHour = pricing.PeakSlots > 0 && pricing.NormalSlots == 0
                ? court.PeakPricePerHour ?? court.PricePerHour
                : court.PricePerHour,
            TotalPrice = pricing.Subtotal,
            NormalSlots = pricing.NormalSlots,
            PeakSlots = pricing.PeakSlots,
            NormalSubtotal = pricing.NormalSubtotal,
            PeakSubtotal = pricing.PeakSubtotal,
            NormalPricePerSlot = court.PricePerHour,
            PeakPricePerSlot = court.PeakPricePerHour
        };

        await _reservationRepository.AddAsync(reservation);
        await _unitOfWork.SaveChangesAsync();
        return Result.Ok(reservation);
    }


}
