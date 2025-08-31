using Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence.Seeders;

public class ReservationSeeder : BaseSeeder
{
    private readonly IReservationRepository _reservationRepository;

    public ReservationSeeder(
        IReservationRepository reservationRepository,
        ILogger<ReservationSeeder> logger) : base(logger)
    {
        _reservationRepository = reservationRepository;
    }

    public override int Order => 5; // Fifth to be executed (after Courts)

    public override async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        LogInfo("Starting reservations seeding...");

        var reservations = GetTestReservations();

        foreach (var reservation in reservations)
        {
            var existing = await _reservationRepository.GetByIdAsync(reservation.Id, cancellationToken);
            if (existing != null)
            {
                LogInfo($"Reservation already exists: {reservation.Id}");
                continue;
            }

            await _reservationRepository.AddAsync(reservation, cancellationToken);
            LogInfo($"Created reservation for court {reservation.CourtId} by user {reservation.UserId}");
        }

        LogInfo("Reservations seeding completed.");
    }

    private List<Reservation> GetTestReservations()
    {
        var baseDate = DateTime.UtcNow.AddDays(1); // Start from tomorrow
        var reservations = new List<Reservation>();

        // Past reservations (for testing history)
        var pastDate = DateTime.UtcNow.AddDays(-7);
        
        // SportHub Central Arena - Past reservations
        reservations.Add(new Reservation
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            CourtId = Guid.Parse("11111111-1111-1111-1111-111111111112"), // Main Football Court
            UserId = Guid.Parse("44444444-4444-4444-4444-444444444444"), // Anna Brown
            StartTimeUtc = pastDate.AddHours(14),
            EndTimeUtc = pastDate.AddHours(15)
        });

        reservations.Add(new Reservation
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111113"),
            CourtId = Guid.Parse("11111111-1111-1111-1111-111111111114"), // Basketball Court A
            UserId = Guid.Parse("55555555-5555-5555-5555-555555555555"), // Peter Davis
            StartTimeUtc = pastDate.AddHours(16),
            EndTimeUtc = pastDate.AddHours(17).AddMinutes(30)
        });

        // Future reservations for today and next days
        
        // Today's reservations
        var today = DateTime.UtcNow.Date;
        
        reservations.Add(new Reservation
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222221"),
            CourtId = Guid.Parse("22222222-2222-2222-2222-222222222222"), // Tennis Court 1
            UserId = Guid.Parse("66666666-6666-6666-6666-666666666666"), // Lucy Miller
            StartTimeUtc = today.AddHours(18),
            EndTimeUtc = today.AddHours(19)
        });

        reservations.Add(new Reservation
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222223"),
            CourtId = Guid.Parse("22222222-2222-2222-2222-222222222224"), // Padel Court 1
            UserId = Guid.Parse("77777777-7777-7777-7777-777777777777"), // Robert Wilson
            StartTimeUtc = today.AddHours(20),
            EndTimeUtc = today.AddHours(21).AddMinutes(30)
        });

        // Tomorrow's reservations
        var tomorrow = baseDate;
        
        reservations.Add(new Reservation
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333331"),
            CourtId = Guid.Parse("33333333-3333-3333-3333-333333333332"), // Futsal Court 1
            UserId = Guid.Parse("44444444-4444-4444-4444-444444444444"), // Anna Brown
            StartTimeUtc = tomorrow.AddHours(19),
            EndTimeUtc = tomorrow.AddHours(20)
        });

        reservations.Add(new Reservation
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            CourtId = Guid.Parse("33333333-3333-3333-3333-333333333334"), // Futsal Court 2
            UserId = Guid.Parse("88888888-8888-8888-8888-888888888888"), // Emily Moore
            StartTimeUtc = tomorrow.AddHours(20),
            EndTimeUtc = tomorrow.AddHours(21)
        });

        // Day after tomorrow
        var dayAfter = baseDate.AddDays(1);
        
        reservations.Add(new Reservation
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111115"),
            CourtId = Guid.Parse("11111111-1111-1111-1111-111111111116"), // Volleyball Court A
            UserId = Guid.Parse("55555555-5555-5555-5555-555555555555"), // Peter Davis
            StartTimeUtc = dayAfter.AddHours(15),
            EndTimeUtc = dayAfter.AddHours(16)
        });

        reservations.Add(new Reservation
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222225"),
            CourtId = Guid.Parse("22222222-2222-2222-2222-222222222226"), // Tennis Court 2
            UserId = Guid.Parse("66666666-6666-6666-6666-666666666666"), // Lucy Miller
            StartTimeUtc = dayAfter.AddHours(17),
            EndTimeUtc = dayAfter.AddHours(18)
        });

        // Weekend reservations (more busy)
        var saturday = baseDate.AddDays(5 - (int)baseDate.DayOfWeek); // Next Saturday
        
        reservations.Add(new Reservation
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333335"),
            CourtId = Guid.Parse("33333333-3333-3333-3333-333333333336"), // Futsal Court 3
            UserId = Guid.Parse("77777777-7777-7777-7777-777777777777"), // Robert Wilson
            StartTimeUtc = saturday.AddHours(10),
            EndTimeUtc = saturday.AddHours(11)
        });

        reservations.Add(new Reservation
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333337"),
            CourtId = Guid.Parse("33333333-3333-3333-3333-333333333338"), // Futsal Court 4
            UserId = Guid.Parse("88888888-8888-8888-8888-888888888888"), // Emily Moore
            StartTimeUtc = saturday.AddHours(11),
            EndTimeUtc = saturday.AddHours(12)
        });

        return reservations;
    }
}
