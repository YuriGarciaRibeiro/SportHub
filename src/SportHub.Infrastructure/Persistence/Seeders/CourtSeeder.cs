using Domain.Entities;
using Microsoft.Extensions.Logging;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Seeders;

public class CourtSeeder : BaseSeeder
{
    private readonly IEstablishmentsRepository _establishmentsRepository;
    private readonly ICourtsRepository _courtsRepository;
    private readonly ISportsRepository _sportsRepository;
    private readonly ApplicationDbContext _dbContext;

    public CourtSeeder(
        IEstablishmentsRepository establishmentsRepository,
        ICourtsRepository courtsRepository,
        ISportsRepository sportsRepository,
        ApplicationDbContext dbContext,
        ILogger<CourtSeeder> logger) : base(logger)
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

        // SportHub Arena Central - Courts
        var centralArena = establishmentsList.FirstOrDefault(e => e.Name == "SportHub Arena Central");
        if (centralArena != null)
        {
            var football = sportsList.FirstOrDefault(s => s.Name == "Football");
            var basketball = sportsList.FirstOrDefault(s => s.Name == "Basketball");

            if (football != null)
            {
                courtsData.Add((new Court
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111112"),
                    Name = "Field 1 - Official",
                    EstablishmentId = centralArena.Id,
                    SlotDurationMinutes = 60,
                    MinBookingSlots = 1,
                    MaxBookingSlots = 3,
                    OpeningTime = new TimeOnly(6, 0),
                    ClosingTime = new TimeOnly(22, 0),
                    PricePerSlot = 50.00m
                }, new List<Guid> { football.Id }));
            }

            if (basketball != null)
            {
                courtsData.Add((new Court
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111114"),
                    Name = "Court A - Indoor",
                    EstablishmentId = centralArena.Id,
                    SlotDurationMinutes = 90,
                    MinBookingSlots = 1,
                    MaxBookingSlots = 2,
                    OpeningTime = new TimeOnly(7, 0),
                    ClosingTime = new TimeOnly(21, 0),
                    PricePerSlot = 75.00m
                }, new List<Guid> { basketball.Id }));
            }

            var volleyball = sportsList.FirstOrDefault(s => s.Name == "Volleyball");
            if (volleyball != null)
            {
                courtsData.Add((new Court
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111116"),
                    Name = "Volleyball Court A",
                    EstablishmentId = centralArena.Id,
                    SlotDurationMinutes = 60,
                    MinBookingSlots = 1,
                    MaxBookingSlots = 2,
                    OpeningTime = new TimeOnly(8, 0),
                    ClosingTime = new TimeOnly(22, 0),
                    PricePerSlot = 50.00m
                }, new List<Guid> { volleyball.Id }));
            }
        }

        // Club Premium Atalaia - Courts
        var premiumClub = establishmentsList.FirstOrDefault(e => e.Name == "Club Premium Atalaia");
        if (premiumClub != null)
        {
            var tennis = sportsList.FirstOrDefault(s => s.Name == "Tennis");
            var padel = sportsList.FirstOrDefault(s => s.Name == "Padel");

            if (tennis != null)
            {
                courtsData.Add((new Court
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Name = "Tennis Court 1",
                    EstablishmentId = premiumClub.Id,
                    SlotDurationMinutes = 60,
                    MinBookingSlots = 1,
                    MaxBookingSlots = 2,
                    OpeningTime = new TimeOnly(7, 0),
                    ClosingTime = new TimeOnly(22, 0),
                    PricePerSlot = 40.00m
                }, new List<Guid> { tennis.Id }));

                courtsData.Add((new Court
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222226"),
                    Name = "Tennis Court 2",
                    EstablishmentId = premiumClub.Id,
                    SlotDurationMinutes = 60,
                    MinBookingSlots = 1,
                    MaxBookingSlots = 2,
                    OpeningTime = new TimeOnly(7, 0),
                    ClosingTime = new TimeOnly(22, 0),
                    PricePerSlot = 40.00m
                }, new List<Guid> { tennis.Id }));
            }

            if (padel != null)
            {
                courtsData.Add((new Court
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222224"),
                    Name = "Padel Court 1",
                    EstablishmentId = premiumClub.Id,
                    SlotDurationMinutes = 90,
                    MinBookingSlots = 1,
                    MaxBookingSlots = 2,
                    OpeningTime = new TimeOnly(8, 0),
                    ClosingTime = new TimeOnly(21, 0),
                    PricePerSlot = 60.00m
                }, new List<Guid> { padel.Id }));
            }
        }

        // Centro de Futsal Sergipe - Courts
        var futsalCenter = establishmentsList.FirstOrDefault(e => e.Name == "Centro de Futsal Sergipe");
        if (futsalCenter != null)
        {
            var futsal = sportsList.FirstOrDefault(s => s.Name == "Futsal");

            if (futsal != null)
            {
                courtsData.Add((new Court
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333332"),
                    Name = "Futsal Court 1",
                    EstablishmentId = futsalCenter.Id,
                    SlotDurationMinutes = 60,
                    MinBookingSlots = 1,
                    MaxBookingSlots = 2,
                    OpeningTime = new TimeOnly(6, 0),
                    ClosingTime = new TimeOnly(23, 0),
                    PricePerSlot = 30.00m
                }, new List<Guid> { futsal.Id }));

                courtsData.Add((new Court
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333334"),
                    Name = "Futsal Court 2",
                    EstablishmentId = futsalCenter.Id,
                    SlotDurationMinutes = 60,
                    MinBookingSlots = 1,
                    MaxBookingSlots = 2,
                    OpeningTime = new TimeOnly(6, 0),
                    ClosingTime = new TimeOnly(23, 0),
                    PricePerSlot = 30.00m
                }, new List<Guid> { futsal.Id }));

                courtsData.Add((new Court
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333336"),
                    Name = "Futsal Court 3",
                    EstablishmentId = futsalCenter.Id,
                    SlotDurationMinutes = 60,
                    MinBookingSlots = 1,
                    MaxBookingSlots = 2,
                    OpeningTime = new TimeOnly(6, 0),
                    ClosingTime = new TimeOnly(23, 0),
                    PricePerSlot = 30.00m
                }, new List<Guid> { futsal.Id }));

                courtsData.Add((new Court
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333338"),
                    Name = "Futsal Court 4",
                    EstablishmentId = futsalCenter.Id,
                    SlotDurationMinutes = 60,
                    MinBookingSlots = 1,
                    MaxBookingSlots = 2,
                    OpeningTime = new TimeOnly(6, 0),
                    ClosingTime = new TimeOnly(23, 0),
                    PricePerSlot = 30.00m
                }, new List<Guid> { futsal.Id }));
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
