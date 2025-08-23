using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence.Seeders;

public class CourtSeeder : BaseSeeder
{
    private readonly IEstablishmentsRepository _establishmentsRepository;
    private readonly ICourtsRepository _courtsRepository;
    private readonly ISportsRepository _sportsRepository;

    public CourtSeeder(
        IEstablishmentsRepository establishmentsRepository,
        ICourtsRepository courtsRepository,
        ISportsRepository sportsRepository,
        ILogger<CourtSeeder> logger) : base(logger)
    {
        _establishmentsRepository = establishmentsRepository;
        _courtsRepository = courtsRepository;
        _sportsRepository = sportsRepository;
    }

    public override int Order => 4;

    public override async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        LogInfo("Starting courts seeding...");

        var existingCourts = await _courtsRepository.GetAllAsync(cancellationToken);
        if (existingCourts?.Any() == true)
        {
            LogInfo($"Courts already exist ({existingCourts.Count()} found). Skipping seeding.");
            return;
        }

        // Get establishments and sports
        var establishments = await _establishmentsRepository.GetAllAsync(cancellationToken);
        var sports = await _sportsRepository.GetAllAsync(cancellationToken);

        if (!establishments?.Any() == true)
        {
            LogInfo("No establishments found. Cannot seed courts.");
            return;
        }

        if (!sports?.Any() == true)
        {
            LogInfo("No sports found. Cannot seed courts.");
            return;
        }

        var establishmentsList = establishments!.ToList();
        var sportsList = sports!.ToList();

        var courtsToAdd = new List<Court>();

        // SportHub Central Arena - Courts
        var centralArena = establishmentsList.FirstOrDefault(e => e.Name == "SportHub Central Arena");
        if (centralArena != null)
        {
            var football = sportsList.FirstOrDefault(s => s.Name == "Football");
            var basketball = sportsList.FirstOrDefault(s => s.Name == "Basketball");

            if (football != null)
            {
                courtsToAdd.Add(new Court
                {
                    Id = Guid.NewGuid(),
                    Name = "Field 1 - Official",
                    EstablishmentId = centralArena.Id,
                    SlotDurationMinutes = 60,
                    MinBookingSlots = 1,
                    MaxBookingSlots = 3,
                    OpeningTime = new TimeOnly(6, 0),
                    ClosingTime = new TimeOnly(22, 0),
                    TimeZone = "America/Maceio"
                });
            }

            if (basketball != null)
            {
                courtsToAdd.Add(new Court
                {
                    Id = Guid.NewGuid(),
                    Name = "Court A - Indoor",
                    EstablishmentId = centralArena.Id,
                    SlotDurationMinutes = 90,
                    MinBookingSlots = 1,
                    MaxBookingSlots = 2,
                    OpeningTime = new TimeOnly(7, 0),
                    ClosingTime = new TimeOnly(21, 0),
                    TimeZone = "America/Maceio"
                });
            }
        }

        // Add all courts
        foreach (var court in courtsToAdd)
        {
            await _courtsRepository.AddAsync(court, cancellationToken);
            LogInfo($"Created court: {court.Name}");
        }

        LogInfo($"Courts seeding completed. Added {courtsToAdd.Count} courts.");
    }
}
