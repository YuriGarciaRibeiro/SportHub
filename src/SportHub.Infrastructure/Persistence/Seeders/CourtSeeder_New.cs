using Domain.Entities;
using Microsoft.Extensions.Logging;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Seeders;

public class CourtSeederNew : BaseSeeder
{
    private readonly IEstablishmentsRepository _establishmentsRepository;
    private readonly ICourtsRepository _courtsRepository;
    private readonly ISportsRepository _sportsRepository;
    private readonly ApplicationDbContext _dbContext;

    public CourtSeederNew(
        IEstablishmentsRepository establishmentsRepository,
        ICourtsRepository courtsRepository,
        ISportsRepository sportsRepository,
        ApplicationDbContext dbContext,
        ILogger<CourtSeederNew> logger) : base(logger)
    {
        _establishmentsRepository = establishmentsRepository;
        _courtsRepository = courtsRepository;
        _sportsRepository = sportsRepository;
        _dbContext = dbContext;
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

        var courtsData = new List<(Court Court, List<Guid> SportIds)>();

        // SportHub Central Arena - Courts
        var centralArena = establishmentsList.FirstOrDefault(e => e.Name == "SportHub Central Arena");
        if (centralArena != null)
        {
            var football = sportsList.FirstOrDefault(s => s.Name == "Football");
            var basketball = sportsList.FirstOrDefault(s => s.Name == "Basketball");

            if (football != null)
            {
                courtsData.Add((new Court
                {
                    Id = Guid.NewGuid(),
                    Name = "Field 1 - Official",
                    EstablishmentId = centralArena.Id,
                    SlotDurationMinutes = 60,
                    MinBookingSlots = 1,
                    MaxBookingSlots = 3,
                    OpeningTime = new TimeOnly(6, 0),
                    ClosingTime = new TimeOnly(22, 0),
                    TimeZone = "America/Maceio",
                    PricePerSlot = 50.00m
                }, new List<Guid> { football.Id }));
            }

            if (basketball != null)
            {
                courtsData.Add((new Court
                {
                    Id = Guid.NewGuid(),
                    Name = "Court A - Indoor",
                    EstablishmentId = centralArena.Id,
                    SlotDurationMinutes = 90,
                    MinBookingSlots = 1,
                    MaxBookingSlots = 2,
                    OpeningTime = new TimeOnly(7, 0),
                    ClosingTime = new TimeOnly(21, 0),
                    TimeZone = "America/Maceio",
                    PricePerSlot = 75.00m
                }, new List<Guid> { basketball.Id }));
            }
        }

        // Premium Athletic Club - Courts
        var premiumClub = establishmentsList.FirstOrDefault(e => e.Name == "Premium Athletic Club");
        if (premiumClub != null)
        {
            var tennis = sportsList.FirstOrDefault(s => s.Name == "Tennis");
            var padel = sportsList.FirstOrDefault(s => s.Name == "Padel");

            if (tennis != null)
            {
                courtsData.Add((new Court
                {
                    Id = Guid.NewGuid(),
                    Name = "Tennis Court 1",
                    EstablishmentId = premiumClub.Id,
                    SlotDurationMinutes = 60,
                    MinBookingSlots = 1,
                    MaxBookingSlots = 2,
                    OpeningTime = new TimeOnly(7, 0),
                    ClosingTime = new TimeOnly(22, 0),
                    TimeZone = "America/Maceio",
                    PricePerSlot = 40.00m
                }, new List<Guid> { tennis.Id }));

                courtsData.Add((new Court
                {
                    Id = Guid.NewGuid(),
                    Name = "Tennis Court 2",
                    EstablishmentId = premiumClub.Id,
                    SlotDurationMinutes = 60,
                    MinBookingSlots = 1,
                    MaxBookingSlots = 2,
                    OpeningTime = new TimeOnly(7, 0),
                    ClosingTime = new TimeOnly(22, 0),
                    TimeZone = "America/Maceio",
                    PricePerSlot = 40.00m
                }, new List<Guid> { tennis.Id }));
            }

            if (padel != null)
            {
                courtsData.Add((new Court
                {
                    Id = Guid.NewGuid(),
                    Name = "Padel Court 1",
                    EstablishmentId = premiumClub.Id,
                    SlotDurationMinutes = 90,
                    MinBookingSlots = 1,
                    MaxBookingSlots = 2,
                    OpeningTime = new TimeOnly(8, 0),
                    ClosingTime = new TimeOnly(21, 0),
                    TimeZone = "America/Maceio",
                    PricePerSlot = 60.00m
                }, new List<Guid> { padel.Id }));
            }
        }

        // Futsal Mania Center - Courts
        var futsalCenter = establishmentsList.FirstOrDefault(e => e.Name == "Futsal Mania Center");
        if (futsalCenter != null)
        {
            var futsal = sportsList.FirstOrDefault(s => s.Name == "Futsal");

            if (futsal != null)
            {
                for (int i = 1; i <= 4; i++)
                {
                    courtsData.Add((new Court
                    {
                        Id = Guid.NewGuid(),
                        Name = $"Futsal Court {i}",
                        EstablishmentId = futsalCenter.Id,
                        SlotDurationMinutes = 60,
                        MinBookingSlots = 1,
                        MaxBookingSlots = 2,
                        OpeningTime = new TimeOnly(6, 0),
                        ClosingTime = new TimeOnly(23, 0),
                        TimeZone = "America/Maceio",
                        PricePerSlot = 30.00m
                    }, new List<Guid> { futsal.Id }));
                }
            }
        }

        // Add courts and associate sports
        foreach (var (court, sportIds) in courtsData)
        {
            // Add court first
            await _courtsRepository.AddAsync(court, cancellationToken);
            LogInfo($"Created court: {court.Name}");

            // Now associate sports using DbContext directly to avoid tracking conflicts
            if (sportIds.Any())
            {
                var sportsToAssociate = await _dbContext.Sports
                    .Where(s => sportIds.Contains(s.Id))
                    .ToListAsync(cancellationToken);

                var courtEntity = await _dbContext.Courts
                    .Include(c => c.Sports)
                    .FirstOrDefaultAsync(c => c.Id == court.Id, cancellationToken);

                if (courtEntity != null)
                {
                    foreach (var sport in sportsToAssociate)
                    {
                        courtEntity.Sports.Add(sport);
                    }
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    LogInfo($"Associated {sportsToAssociate.Count} sport(s) to court: {court.Name}");
                }
            }
        }

        LogInfo($"Courts seeding completed. Added {courtsData.Count} courts.");
    }
}
