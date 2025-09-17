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

        // ============ NOVOS ESTABELECIMENTOS - QUADRAS ============

        // Arena Beach Tennis Sergipe
        var beachTennisArena = establishmentsList.FirstOrDefault(e => e.Name == "Arena Beach Tennis Sergipe");
        if (beachTennisArena != null)
        {
            var beachTennis = sportsList.FirstOrDefault(s => s.Name == "Beach Tennis");
            if (beachTennis != null)
            {
                for (int i = 1; i <= 6; i++)
                {
                    courtsData.Add((new Court
                    {
                        Id = Guid.Parse($"E4444444-4444-4444-4444-44444444444{i}"),
                        Name = $"Beach Tennis Court {i}",
                        EstablishmentId = beachTennisArena.Id,
                        SlotDurationMinutes = 60,
                        MinBookingSlots = 1,
                        MaxBookingSlots = 2,
                        OpeningTime = new TimeOnly(7, 0),
                        ClosingTime = new TimeOnly(19, 0),
                        PricePerSlot = 35.00m
                    }, new List<Guid> { beachTennis.Id }));
                }
            }
        }

        // Quadra do Bairro 13 de Julho
        var quadra13Julho = establishmentsList.FirstOrDefault(e => e.Name == "Quadra do Bairro 13 de Julho");
        if (quadra13Julho != null)
        {
            var football = sportsList.FirstOrDefault(s => s.Name == "Football");
            if (football != null)
            {
                courtsData.Add((new Court
                {
                    Id = Guid.Parse("E5555555-5555-5555-5555-555555555551"),
                    Name = "Campo Principal",
                    EstablishmentId = quadra13Julho.Id,
                    SlotDurationMinutes = 60,
                    MinBookingSlots = 1,
                    MaxBookingSlots = 2,
                    OpeningTime = new TimeOnly(6, 0),
                    ClosingTime = new TimeOnly(22, 0),
                    PricePerSlot = 25.00m
                }, new List<Guid> { football.Id }));
            }
        }

        // Handebol Sergipe
        var handebolSergipe = establishmentsList.FirstOrDefault(e => e.Name == "Handebol Sergipe");
        if (handebolSergipe != null)
        {
            var handball = sportsList.FirstOrDefault(s => s.Name == "Handball");
            if (handball != null)
            {
                for (int i = 1; i <= 2; i++)
                {
                    courtsData.Add((new Court
                    {
                        Id = Guid.Parse($"E6666666-6666-6666-6666-66666666666{i}"),
                        Name = $"Quadra de Handebol {i}",
                        EstablishmentId = handebolSergipe.Id,
                        SlotDurationMinutes = 90,
                        MinBookingSlots = 1,
                        MaxBookingSlots = 2,
                        OpeningTime = new TimeOnly(7, 0),
                        ClosingTime = new TimeOnly(21, 0),
                        PricePerSlot = 40.00m
                    }, new List<Guid> { handball.Id }));
                }
            }
        }

        // Academia Multi Sport
        var academiaMultiSport = establishmentsList.FirstOrDefault(e => e.Name == "Academia Multi Sport");
        if (academiaMultiSport != null)
        {
            var basketball = sportsList.FirstOrDefault(s => s.Name == "Basketball");
            var volleyball = sportsList.FirstOrDefault(s => s.Name == "Volleyball");
            var futsal = sportsList.FirstOrDefault(s => s.Name == "Futsal");

            if (basketball != null)
            {
                courtsData.Add((new Court
                {
                    Id = Guid.Parse("E7777777-7777-7777-7777-777777777771"),
                    Name = "Quadra de Basquete",
                    EstablishmentId = academiaMultiSport.Id,
                    SlotDurationMinutes = 90,
                    MinBookingSlots = 1,
                    MaxBookingSlots = 2,
                    OpeningTime = new TimeOnly(6, 0),
                    ClosingTime = new TimeOnly(22, 0),
                    PricePerSlot = 45.00m
                }, new List<Guid> { basketball.Id }));
            }

            if (volleyball != null)
            {
                courtsData.Add((new Court
                {
                    Id = Guid.Parse("E7777777-7777-7777-7777-777777777772"),
                    Name = "Quadra de Vôlei",
                    EstablishmentId = academiaMultiSport.Id,
                    SlotDurationMinutes = 60,
                    MinBookingSlots = 1,
                    MaxBookingSlots = 2,
                    OpeningTime = new TimeOnly(6, 0),
                    ClosingTime = new TimeOnly(22, 0),
                    PricePerSlot = 40.00m
                }, new List<Guid> { volleyball.Id }));
            }

            if (futsal != null)
            {
                courtsData.Add((new Court
                {
                    Id = Guid.Parse("E7777777-7777-7777-7777-777777777773"),
                    Name = "Quadra de Futsal",
                    EstablishmentId = academiaMultiSport.Id,
                    SlotDurationMinutes = 60,
                    MinBookingSlots = 1,
                    MaxBookingSlots = 2,
                    OpeningTime = new TimeOnly(6, 0),
                    ClosingTime = new TimeOnly(22, 0),
                    PricePerSlot = 35.00m
                }, new List<Guid> { futsal.Id }));
            }
        }

        // Tennis Club Jardins
        var tennisClubJardins = establishmentsList.FirstOrDefault(e => e.Name == "Tennis Club Jardins");
        if (tennisClubJardins != null)
        {
            var tennis = sportsList.FirstOrDefault(s => s.Name == "Tennis");
            if (tennis != null)
            {
                for (int i = 1; i <= 8; i++)
                {
                    courtsData.Add((new Court
                    {
                        Id = Guid.Parse($"E8888888-8888-8888-8888-88888888888{i}"),
                        Name = $"Quadra de Tênis {i}",
                        EstablishmentId = tennisClubJardins.Id,
                        SlotDurationMinutes = 60,
                        MinBookingSlots = 1,
                        MaxBookingSlots = 2,
                        OpeningTime = new TimeOnly(6, 0),
                        ClosingTime = new TimeOnly(22, 0),
                        PricePerSlot = 50.00m
                    }, new List<Guid> { tennis.Id }));
                }
            }
        }

        // Esporte Clube São Conrado
        var saoConrado = establishmentsList.FirstOrDefault(e => e.Name == "Esporte Clube São Conrado");
        if (saoConrado != null)
        {
            var football = sportsList.FirstOrDefault(s => s.Name == "Football");
            var basketball = sportsList.FirstOrDefault(s => s.Name == "Basketball");
            var volleyball = sportsList.FirstOrDefault(s => s.Name == "Volleyball");

            if (football != null)
            {
                courtsData.Add((new Court
                {
                    Id = Guid.Parse("E9999999-9999-9999-9999-999999999991"),
                    Name = "Campo de Futebol Principal",
                    EstablishmentId = saoConrado.Id,
                    SlotDurationMinutes = 90,
                    MinBookingSlots = 1,
                    MaxBookingSlots = 2,
                    OpeningTime = new TimeOnly(6, 0),
                    ClosingTime = new TimeOnly(22, 0),
                    PricePerSlot = 60.00m
                }, new List<Guid> { football.Id }));
            }

            if (basketball != null)
            {
                courtsData.Add((new Court
                {
                    Id = Guid.Parse("E9999999-9999-9999-9999-999999999992"),
                    Name = "Quadra de Basquete Coberta",
                    EstablishmentId = saoConrado.Id,
                    SlotDurationMinutes = 90,
                    MinBookingSlots = 1,
                    MaxBookingSlots = 2,
                    OpeningTime = new TimeOnly(7, 0),
                    ClosingTime = new TimeOnly(21, 0),
                    PricePerSlot = 55.00m
                }, new List<Guid> { basketball.Id }));
            }

            if (volleyball != null)
            {
                courtsData.Add((new Court
                {
                    Id = Guid.Parse("E9999999-9999-9999-9999-999999999993"),
                    Name = "Quadra de Vôlei",
                    EstablishmentId = saoConrado.Id,
                    SlotDurationMinutes = 60,
                    MinBookingSlots = 1,
                    MaxBookingSlots = 2,
                    OpeningTime = new TimeOnly(7, 0),
                    ClosingTime = new TimeOnly(21, 0),
                    PricePerSlot = 45.00m
                }, new List<Guid> { volleyball.Id }));
            }
        }

        // Futsal Arena Inácio Barbosa
        var futsalInacioBarbosa = establishmentsList.FirstOrDefault(e => e.Name == "Futsal Arena Inácio Barbosa");
        if (futsalInacioBarbosa != null)
        {
            var futsal = sportsList.FirstOrDefault(s => s.Name == "Futsal");
            if (futsal != null)
            {
                for (int i = 1; i <= 2; i++)
                {
                    courtsData.Add((new Court
                    {
                        Id = Guid.Parse($"EA111111-1111-1111-1111-11111111111{i}"),
                        Name = $"Quadra de Futsal {i}",
                        EstablishmentId = futsalInacioBarbosa.Id,
                        SlotDurationMinutes = 60,
                        MinBookingSlots = 1,
                        MaxBookingSlots = 2,
                        OpeningTime = new TimeOnly(6, 0),
                        ClosingTime = new TimeOnly(23, 0),
                        PricePerSlot = 28.00m
                    }, new List<Guid> { futsal.Id }));
                }
            }
        }

        // Padel Center Coroa do Meio
        var padelCenter = establishmentsList.FirstOrDefault(e => e.Name == "Padel Center Coroa do Meio");
        if (padelCenter != null)
        {
            var padel = sportsList.FirstOrDefault(s => s.Name == "Padel");
            if (padel != null)
            {
                for (int i = 1; i <= 4; i++)
                {
                    courtsData.Add((new Court
                    {
                        Id = Guid.Parse($"EA222222-2222-2222-2222-22222222222{i}"),
                        Name = $"Quadra de Padel {i}",
                        EstablishmentId = padelCenter.Id,
                        SlotDurationMinutes = 90,
                        MinBookingSlots = 1,
                        MaxBookingSlots = 2,
                        OpeningTime = new TimeOnly(7, 0),
                        ClosingTime = new TimeOnly(22, 0),
                        PricePerSlot = 70.00m
                    }, new List<Guid> { padel.Id }));
                }
            }
        }

        // Quadra Poliesportiva Bugio
        var quadraBugio = establishmentsList.FirstOrDefault(e => e.Name == "Quadra Poliesportiva Bugio");
        if (quadraBugio != null)
        {
            var football = sportsList.FirstOrDefault(s => s.Name == "Football");
            var basketball = sportsList.FirstOrDefault(s => s.Name == "Basketball");

            if (football != null)
            {
                courtsData.Add((new Court
                {
                    Id = Guid.Parse("EA333333-3333-3333-3333-333333333331"),
                    Name = "Campo de Futebol Society",
                    EstablishmentId = quadraBugio.Id,
                    SlotDurationMinutes = 60,
                    MinBookingSlots = 1,
                    MaxBookingSlots = 2,
                    OpeningTime = new TimeOnly(6, 0),
                    ClosingTime = new TimeOnly(22, 0),
                    PricePerSlot = 22.00m
                }, new List<Guid> { football.Id }));
            }

            if (basketball != null)
            {
                courtsData.Add((new Court
                {
                    Id = Guid.Parse("EA333333-3333-3333-3333-333333333332"),
                    Name = "Quadra de Basquete",
                    EstablishmentId = quadraBugio.Id,
                    SlotDurationMinutes = 90,
                    MinBookingSlots = 1,
                    MaxBookingSlots = 2,
                    OpeningTime = new TimeOnly(6, 0),
                    ClosingTime = new TimeOnly(22, 0),
                    PricePerSlot = 25.00m
                }, new List<Guid> { basketball.Id }));
            }
        }

        // Vôlei de Praia Atalaia
        var voleiPraia = establishmentsList.FirstOrDefault(e => e.Name == "Vôlei de Praia Atalaia");
        if (voleiPraia != null)
        {
            var volleyball = sportsList.FirstOrDefault(s => s.Name == "Volleyball");
            if (volleyball != null)
            {
                for (int i = 1; i <= 3; i++)
                {
                    courtsData.Add((new Court
                    {
                        Id = Guid.Parse($"EA444444-4444-4444-4444-44444444444{i}"),
                        Name = $"Quadra de Vôlei de Praia {i}",
                        EstablishmentId = voleiPraia.Id,
                        SlotDurationMinutes = 60,
                        MinBookingSlots = 1,
                        MaxBookingSlots = 2,
                        OpeningTime = new TimeOnly(7, 0),
                        ClosingTime = new TimeOnly(18, 0),
                        PricePerSlot = 30.00m
                    }, new List<Guid> { volleyball.Id }));
                }
            }
        }

        // Soccer Zone Grageru
        var soccerZone = establishmentsList.FirstOrDefault(e => e.Name == "Soccer Zone Grageru");
        if (soccerZone != null)
        {
            var football = sportsList.FirstOrDefault(s => s.Name == "Football");
            if (football != null)
            {
                for (int i = 1; i <= 3; i++)
                {
                    courtsData.Add((new Court
                    {
                        Id = Guid.Parse($"EA555555-5555-5555-5555-55555555555{i}"),
                        Name = $"Campo Society {i}",
                        EstablishmentId = soccerZone.Id,
                        SlotDurationMinutes = 60,
                        MinBookingSlots = 1,
                        MaxBookingSlots = 2,
                        OpeningTime = new TimeOnly(6, 0),
                        ClosingTime = new TimeOnly(23, 0),
                        PricePerSlot = 35.00m
                    }, new List<Guid> { football.Id }));
                }
            }
        }

        // Centro de Tênis Santos Dumont
        var tenisSantosDumont = establishmentsList.FirstOrDefault(e => e.Name == "Centro de Tênis Santos Dumont");
        if (tenisSantosDumont != null)
        {
            var tennis = sportsList.FirstOrDefault(s => s.Name == "Tennis");
            if (tennis != null)
            {
                for (int i = 1; i <= 6; i++)
                {
                    courtsData.Add((new Court
                    {
                        Id = Guid.Parse($"EA666666-6666-6666-6666-66666666666{i}"),
                        Name = $"Quadra de Tênis {i}",
                        EstablishmentId = tenisSantosDumont.Id,
                        SlotDurationMinutes = 60,
                        MinBookingSlots = 1,
                        MaxBookingSlots = 2,
                        OpeningTime = new TimeOnly(6, 0),
                        ClosingTime = new TimeOnly(21, 0),
                        PricePerSlot = 45.00m
                    }, new List<Guid> { tennis.Id }));
                }
            }
        }

        // Arena Multi Sport Ponto Novo
        var arenaPontoNovo = establishmentsList.FirstOrDefault(e => e.Name == "Arena Multi Sport Ponto Novo");
        if (arenaPontoNovo != null)
        {
            var basketball = sportsList.FirstOrDefault(s => s.Name == "Basketball");
            var volleyball = sportsList.FirstOrDefault(s => s.Name == "Volleyball");
            var futsal = sportsList.FirstOrDefault(s => s.Name == "Futsal");

            if (basketball != null)
            {
                courtsData.Add((new Court
                {
                    Id = Guid.Parse("EA777777-7777-7777-7777-777777777771"),
                    Name = "Quadra de Basquete Premium",
                    EstablishmentId = arenaPontoNovo.Id,
                    SlotDurationMinutes = 90,
                    MinBookingSlots = 1,
                    MaxBookingSlots = 2,
                    OpeningTime = new TimeOnly(6, 0),
                    ClosingTime = new TimeOnly(22, 0),
                    PricePerSlot = 50.00m
                }, new List<Guid> { basketball.Id }));
            }

            if (volleyball != null)
            {
                courtsData.Add((new Court
                {
                    Id = Guid.Parse("EA777777-7777-7777-7777-777777777772"),
                    Name = "Quadra de Vôlei Premium",
                    EstablishmentId = arenaPontoNovo.Id,
                    SlotDurationMinutes = 60,
                    MinBookingSlots = 1,
                    MaxBookingSlots = 2,
                    OpeningTime = new TimeOnly(6, 0),
                    ClosingTime = new TimeOnly(22, 0),
                    PricePerSlot = 45.00m
                }, new List<Guid> { volleyball.Id }));
            }

            if (futsal != null)
            {
                courtsData.Add((new Court
                {
                    Id = Guid.Parse("EA777777-7777-7777-7777-777777777773"),
                    Name = "Quadra de Futsal Premium",
                    EstablishmentId = arenaPontoNovo.Id,
                    SlotDurationMinutes = 60,
                    MinBookingSlots = 1,
                    MaxBookingSlots = 2,
                    OpeningTime = new TimeOnly(6, 0),
                    ClosingTime = new TimeOnly(22, 0),
                    PricePerSlot = 40.00m
                }, new List<Guid> { futsal.Id }));
            }
        }

        // Handebol Clube Jabotiana
        var handebolJabotiana = establishmentsList.FirstOrDefault(e => e.Name == "Handebol Clube Jabotiana");
        if (handebolJabotiana != null)
        {
            var handball = sportsList.FirstOrDefault(s => s.Name == "Handball");
            if (handball != null)
            {
                for (int i = 1; i <= 2; i++)
                {
                    courtsData.Add((new Court
                    {
                        Id = Guid.Parse($"EA888888-8888-8888-8888-88888888888{i}"),
                        Name = $"Quadra de Handebol Oficial {i}",
                        EstablishmentId = handebolJabotiana.Id,
                        SlotDurationMinutes = 90,
                        MinBookingSlots = 1,
                        MaxBookingSlots = 2,
                        OpeningTime = new TimeOnly(7, 0),
                        ClosingTime = new TimeOnly(21, 0),
                        PricePerSlot = 42.00m
                    }, new List<Guid> { handball.Id }));
                }
            }
        }

        // Beach Tennis Paradise
        var beachTennisParadise = establishmentsList.FirstOrDefault(e => e.Name == "Beach Tennis Paradise");
        if (beachTennisParadise != null)
        {
            var beachTennis = sportsList.FirstOrDefault(s => s.Name == "Beach Tennis");
            if (beachTennis != null)
            {
                for (int i = 1; i <= 8; i++)
                {
                    courtsData.Add((new Court
                    {
                        Id = Guid.Parse($"EA999999-9999-9999-9999-99999999999{i}"),
                        Name = $"Quadra Beach Tennis Paradise {i}",
                        EstablishmentId = beachTennisParadise.Id,
                        SlotDurationMinutes = 60,
                        MinBookingSlots = 1,
                        MaxBookingSlots = 2,
                        OpeningTime = new TimeOnly(7, 0),
                        ClosingTime = new TimeOnly(19, 0),
                        PricePerSlot = 40.00m
                    }, new List<Guid> { beachTennis.Id }));
                }
            }
        }

        // Quadra Popular Industrial
        var quadraIndustrial = establishmentsList.FirstOrDefault(e => e.Name == "Quadra Popular Industrial");
        if (quadraIndustrial != null)
        {
            var football = sportsList.FirstOrDefault(s => s.Name == "Football");
            if (football != null)
            {
                courtsData.Add((new Court
                {
                    Id = Guid.Parse("EB111111-1111-1111-1111-111111111111"),
                    Name = "Campo Popular",
                    EstablishmentId = quadraIndustrial.Id,
                    SlotDurationMinutes = 60,
                    MinBookingSlots = 1,
                    MaxBookingSlots = 2,
                    OpeningTime = new TimeOnly(6, 0),
                    ClosingTime = new TimeOnly(22, 0),
                    PricePerSlot = 20.00m
                }, new List<Guid> { football.Id }));
            }
        }

        // Padel Elite Luzia
        var padelElite = establishmentsList.FirstOrDefault(e => e.Name == "Padel Elite Luzia");
        if (padelElite != null)
        {
            var padel = sportsList.FirstOrDefault(s => s.Name == "Padel");
            if (padel != null)
            {
                for (int i = 1; i <= 6; i++)
                {
                    courtsData.Add((new Court
                    {
                        Id = Guid.Parse($"EB222222-2222-2222-2222-22222222222{i}"),
                        Name = $"Quadra Elite Padel {i}",
                        EstablishmentId = padelElite.Id,
                        SlotDurationMinutes = 90,
                        MinBookingSlots = 1,
                        MaxBookingSlots = 2,
                        OpeningTime = new TimeOnly(7, 0),
                        ClosingTime = new TimeOnly(22, 0),
                        PricePerSlot = 80.00m
                    }, new List<Guid> { padel.Id }));
                }
            }
        }

        // Basquete Center Olaria
        var basqueteOlaria = establishmentsList.FirstOrDefault(e => e.Name == "Basquete Center Olaria");
        if (basqueteOlaria != null)
        {
            var basketball = sportsList.FirstOrDefault(s => s.Name == "Basketball");
            if (basketball != null)
            {
                for (int i = 1; i <= 3; i++)
                {
                    courtsData.Add((new Court
                    {
                        Id = Guid.Parse($"EB333333-3333-3333-3333-33333333333{i}"),
                        Name = $"Quadra de Basquete {i}",
                        EstablishmentId = basqueteOlaria.Id,
                        SlotDurationMinutes = 90,
                        MinBookingSlots = 1,
                        MaxBookingSlots = 2,
                        OpeningTime = new TimeOnly(6, 0),
                        ClosingTime = new TimeOnly(22, 0),
                        PricePerSlot = 40.00m
                    }, new List<Guid> { basketball.Id }));
                }
            }
        }

        // Vôlei Total São José
        var voleiSaoJose = establishmentsList.FirstOrDefault(e => e.Name == "Vôlei Total São José");
        if (voleiSaoJose != null)
        {
            var volleyball = sportsList.FirstOrDefault(s => s.Name == "Volleyball");
            if (volleyball != null)
            {
                for (int i = 1; i <= 4; i++)
                {
                    courtsData.Add((new Court
                    {
                        Id = Guid.Parse($"EB444444-4444-4444-4444-44444444444{i}"),
                        Name = $"Quadra de Vôlei {i}",
                        EstablishmentId = voleiSaoJose.Id,
                        SlotDurationMinutes = 60,
                        MinBookingSlots = 1,
                        MaxBookingSlots = 2,
                        OpeningTime = new TimeOnly(6, 0),
                        ClosingTime = new TimeOnly(22, 0),
                        PricePerSlot = 38.00m
                    }, new List<Guid> { volleyball.Id }));
                }
            }
        }

        // Futsal Arena Cidade Nova
        var futsalCidadeNova = establishmentsList.FirstOrDefault(e => e.Name == "Futsal Arena Cidade Nova");
        if (futsalCidadeNova != null)
        {
            var futsal = sportsList.FirstOrDefault(s => s.Name == "Futsal");
            if (futsal != null)
            {
                for (int i = 1; i <= 2; i++)
                {
                    courtsData.Add((new Court
                    {
                        Id = Guid.Parse($"EB555555-5555-5555-5555-55555555555{i}"),
                        Name = $"Quadra de Futsal Moderna {i}",
                        EstablishmentId = futsalCidadeNova.Id,
                        SlotDurationMinutes = 60,
                        MinBookingSlots = 1,
                        MaxBookingSlots = 2,
                        OpeningTime = new TimeOnly(6, 0),
                        ClosingTime = new TimeOnly(23, 0),
                        PricePerSlot = 32.00m
                    }, new List<Guid> { futsal.Id }));
                }
            }
        }

        // Tennis Academy Aeroporto
        var tennisAeroporto = establishmentsList.FirstOrDefault(e => e.Name == "Tennis Academy Aeroporto");
        if (tennisAeroporto != null)
        {
            var tennis = sportsList.FirstOrDefault(s => s.Name == "Tennis");
            if (tennis != null)
            {
                for (int i = 1; i <= 5; i++)
                {
                    courtsData.Add((new Court
                    {
                        Id = Guid.Parse($"EB666666-6666-6666-6666-66666666666{i}"),
                        Name = $"Quadra Academia {i}",
                        EstablishmentId = tennisAeroporto.Id,
                        SlotDurationMinutes = 60,
                        MinBookingSlots = 1,
                        MaxBookingSlots = 2,
                        OpeningTime = new TimeOnly(6, 0),
                        ClosingTime = new TimeOnly(21, 0),
                        PricePerSlot = 48.00m
                    }, new List<Guid> { tennis.Id }));
                }
            }
        }

        // Complexo Esportivo Zona Sul
        var complexoZonaSul = establishmentsList.FirstOrDefault(e => e.Name == "Complexo Esportivo Zona Sul");
        if (complexoZonaSul != null)
        {
            var futsal = sportsList.FirstOrDefault(s => s.Name == "Futsal");
            var basketball = sportsList.FirstOrDefault(s => s.Name == "Basketball");
            var volleyball = sportsList.FirstOrDefault(s => s.Name == "Volleyball");
            var handball = sportsList.FirstOrDefault(s => s.Name == "Handball");

            if (futsal != null)
            {
                for (int i = 1; i <= 2; i++)
                {
                    courtsData.Add((new Court
                    {
                        Id = Guid.Parse($"EB777777-7777-7777-7777-77777777777{i}"),
                        Name = $"Quadra de Futsal {i}",
                        EstablishmentId = complexoZonaSul.Id,
                        SlotDurationMinutes = 60,
                        MinBookingSlots = 1,
                        MaxBookingSlots = 2,
                        OpeningTime = new TimeOnly(6, 0),
                        ClosingTime = new TimeOnly(23, 0),
                        PricePerSlot = 38.00m
                    }, new List<Guid> { futsal.Id }));
                }
            }

            if (basketball != null)
            {
                courtsData.Add((new Court
                {
                    Id = Guid.Parse("EB777777-7777-7777-7777-777777777773"),
                    Name = "Quadra de Basquete",
                    EstablishmentId = complexoZonaSul.Id,
                    SlotDurationMinutes = 90,
                    MinBookingSlots = 1,
                    MaxBookingSlots = 2,
                    OpeningTime = new TimeOnly(6, 0),
                    ClosingTime = new TimeOnly(22, 0),
                    PricePerSlot = 45.00m
                }, new List<Guid> { basketball.Id }));
            }

            if (volleyball != null)
            {
                courtsData.Add((new Court
                {
                    Id = Guid.Parse("EB777777-7777-7777-7777-777777777774"),
                    Name = "Quadra de Vôlei",
                    EstablishmentId = complexoZonaSul.Id,
                    SlotDurationMinutes = 60,
                    MinBookingSlots = 1,
                    MaxBookingSlots = 2,
                    OpeningTime = new TimeOnly(6, 0),
                    ClosingTime = new TimeOnly(22, 0),
                    PricePerSlot = 40.00m
                }, new List<Guid> { volleyball.Id }));
            }

            if (handball != null)
            {
                courtsData.Add((new Court
                {
                    Id = Guid.Parse("EB777777-7777-7777-7777-777777777775"),
                    Name = "Quadra de Handebol",
                    EstablishmentId = complexoZonaSul.Id,
                    SlotDurationMinutes = 90,
                    MinBookingSlots = 1,
                    MaxBookingSlots = 2,
                    OpeningTime = new TimeOnly(6, 0),
                    ClosingTime = new TimeOnly(22, 0),
                    PricePerSlot = 42.00m
                }, new List<Guid> { handball.Id }));
            }
        }

        // Beach Sports Orla
        var beachSportsOrla = establishmentsList.FirstOrDefault(e => e.Name == "Beach Sports Orla");
        if (beachSportsOrla != null)
        {
            var beachTennis = sportsList.FirstOrDefault(s => s.Name == "Beach Tennis");
            var volleyball = sportsList.FirstOrDefault(s => s.Name == "Volleyball");
            var football = sportsList.FirstOrDefault(s => s.Name == "Football");

            if (beachTennis != null)
            {
                for (int i = 1; i <= 4; i++)
                {
                    courtsData.Add((new Court
                    {
                        Id = Guid.Parse($"EB888888-8888-8888-8888-88888888888{i}"),
                        Name = $"Quadra Beach Tennis {i}",
                        EstablishmentId = beachSportsOrla.Id,
                        SlotDurationMinutes = 60,
                        MinBookingSlots = 1,
                        MaxBookingSlots = 2,
                        OpeningTime = new TimeOnly(7, 0),
                        ClosingTime = new TimeOnly(18, 0),
                        PricePerSlot = 35.00m
                    }, new List<Guid> { beachTennis.Id }));
                }
            }

            if (volleyball != null)
            {
                for (int i = 5; i <= 6; i++)
                {
                    courtsData.Add((new Court
                    {
                        Id = Guid.Parse($"EB888888-8888-8888-8888-88888888888{i}"),
                        Name = $"Quadra Vôlei de Praia {i - 4}",
                        EstablishmentId = beachSportsOrla.Id,
                        SlotDurationMinutes = 60,
                        MinBookingSlots = 1,
                        MaxBookingSlots = 2,
                        OpeningTime = new TimeOnly(7, 0),
                        ClosingTime = new TimeOnly(18, 0),
                        PricePerSlot = 30.00m
                    }, new List<Guid> { volleyball.Id }));
                }
            }

            if (football != null)
            {
                courtsData.Add((new Court
                {
                    Id = Guid.Parse("EB888888-8888-8888-8888-888888888887"),
                    Name = "Campo de Futebol de Areia",
                    EstablishmentId = beachSportsOrla.Id,
                    SlotDurationMinutes = 60,
                    MinBookingSlots = 1,
                    MaxBookingSlots = 2,
                    OpeningTime = new TimeOnly(7, 0),
                    ClosingTime = new TimeOnly(18, 0),
                    PricePerSlot = 40.00m
                }, new List<Guid> { football.Id }));
            }
        }

        // Arena Esportiva América
        var arenaAmerica = establishmentsList.FirstOrDefault(e => e.Name == "Arena Esportiva América");
        if (arenaAmerica != null)
        {
            var football = sportsList.FirstOrDefault(s => s.Name == "Football");
            var futsal = sportsList.FirstOrDefault(s => s.Name == "Futsal");

            if (football != null)
            {
                courtsData.Add((new Court
                {
                    Id = Guid.Parse("EB999999-9999-9999-9999-999999999991"),
                    Name = "Campo Society Principal",
                    EstablishmentId = arenaAmerica.Id,
                    SlotDurationMinutes = 60,
                    MinBookingSlots = 1,
                    MaxBookingSlots = 2,
                    OpeningTime = new TimeOnly(6, 0),
                    ClosingTime = new TimeOnly(23, 0),
                    PricePerSlot = 30.00m
                }, new List<Guid> { football.Id }));
            }

            if (futsal != null)
            {
                courtsData.Add((new Court
                {
                    Id = Guid.Parse("EB999999-9999-9999-9999-999999999992"),
                    Name = "Quadra de Futsal",
                    EstablishmentId = arenaAmerica.Id,
                    SlotDurationMinutes = 60,
                    MinBookingSlots = 1,
                    MaxBookingSlots = 2,
                    OpeningTime = new TimeOnly(6, 0),
                    ClosingTime = new TimeOnly(23, 0),
                    PricePerSlot = 28.00m
                }, new List<Guid> { futsal.Id }));
            }
        }

        // Centro de Treinamento Esportivo
        var centroTreinamento = establishmentsList.FirstOrDefault(e => e.Name == "Centro de Treinamento Esportivo");
        if (centroTreinamento != null)
        {
            var football = sportsList.FirstOrDefault(s => s.Name == "Football");
            var basketball = sportsList.FirstOrDefault(s => s.Name == "Basketball");
            var volleyball = sportsList.FirstOrDefault(s => s.Name == "Volleyball");
            var tennis = sportsList.FirstOrDefault(s => s.Name == "Tennis");
            var futsal = sportsList.FirstOrDefault(s => s.Name == "Futsal");
            var handball = sportsList.FirstOrDefault(s => s.Name == "Handball");
            var padel = sportsList.FirstOrDefault(s => s.Name == "Padel");
            var beachTennis = sportsList.FirstOrDefault(s => s.Name == "Beach Tennis");

            if (football != null)
            {
                courtsData.Add((new Court
                {
                    Id = Guid.Parse("EC111111-1111-1111-1111-111111111111"),
                    Name = "Campo de Futebol Oficial",
                    EstablishmentId = centroTreinamento.Id,
                    SlotDurationMinutes = 90,
                    MinBookingSlots = 1,
                    MaxBookingSlots = 2,
                    OpeningTime = new TimeOnly(6, 0),
                    ClosingTime = new TimeOnly(22, 0),
                    PricePerSlot = 65.00m
                }, new List<Guid> { football.Id }));
            }

            if (basketball != null)
            {
                courtsData.Add((new Court
                {
                    Id = Guid.Parse("EC111111-1111-1111-1111-111111111112"),
                    Name = "Quadra de Basquete Profissional",
                    EstablishmentId = centroTreinamento.Id,
                    SlotDurationMinutes = 90,
                    MinBookingSlots = 1,
                    MaxBookingSlots = 2,
                    OpeningTime = new TimeOnly(6, 0),
                    ClosingTime = new TimeOnly(22, 0),
                    PricePerSlot = 55.00m
                }, new List<Guid> { basketball.Id }));
            }

            if (volleyball != null)
            {
                courtsData.Add((new Court
                {
                    Id = Guid.Parse("EC111111-1111-1111-1111-111111111113"),
                    Name = "Quadra de Vôlei Profissional",
                    EstablishmentId = centroTreinamento.Id,
                    SlotDurationMinutes = 60,
                    MinBookingSlots = 1,
                    MaxBookingSlots = 2,
                    OpeningTime = new TimeOnly(6, 0),
                    ClosingTime = new TimeOnly(22, 0),
                    PricePerSlot = 50.00m
                }, new List<Guid> { volleyball.Id }));
            }

            if (tennis != null)
            {
                for (int i = 4; i <= 5; i++)
                {
                    courtsData.Add((new Court
                    {
                        Id = Guid.Parse($"EC111111-1111-1111-1111-11111111111{i}"),
                        Name = $"Quadra de Tênis Profissional {i - 3}",
                        EstablishmentId = centroTreinamento.Id,
                        SlotDurationMinutes = 60,
                        MinBookingSlots = 1,
                        MaxBookingSlots = 2,
                        OpeningTime = new TimeOnly(6, 0),
                        ClosingTime = new TimeOnly(22, 0),
                        PricePerSlot = 60.00m
                    }, new List<Guid> { tennis.Id }));
                }
            }

            if (futsal != null)
            {
                courtsData.Add((new Court
                {
                    Id = Guid.Parse("EC111111-1111-1111-1111-111111111116"),
                    Name = "Quadra de Futsal Profissional",
                    EstablishmentId = centroTreinamento.Id,
                    SlotDurationMinutes = 60,
                    MinBookingSlots = 1,
                    MaxBookingSlots = 2,
                    OpeningTime = new TimeOnly(6, 0),
                    ClosingTime = new TimeOnly(22, 0),
                    PricePerSlot = 45.00m
                }, new List<Guid> { futsal.Id }));
            }

            if (handball != null)
            {
                courtsData.Add((new Court
                {
                    Id = Guid.Parse("EC111111-1111-1111-1111-111111111117"),
                    Name = "Quadra de Handebol Profissional",
                    EstablishmentId = centroTreinamento.Id,
                    SlotDurationMinutes = 90,
                    MinBookingSlots = 1,
                    MaxBookingSlots = 2,
                    OpeningTime = new TimeOnly(6, 0),
                    ClosingTime = new TimeOnly(22, 0),
                    PricePerSlot = 50.00m
                }, new List<Guid> { handball.Id }));
            }

            if (padel != null)
            {
                courtsData.Add((new Court
                {
                    Id = Guid.Parse("EC111111-1111-1111-1111-111111111118"),
                    Name = "Quadra de Padel Profissional",
                    EstablishmentId = centroTreinamento.Id,
                    SlotDurationMinutes = 90,
                    MinBookingSlots = 1,
                    MaxBookingSlots = 2,
                    OpeningTime = new TimeOnly(6, 0),
                    ClosingTime = new TimeOnly(22, 0),
                    PricePerSlot = 75.00m
                }, new List<Guid> { padel.Id }));
            }

            if (beachTennis != null)
            {
                courtsData.Add((new Court
                {
                    Id = Guid.Parse("EC111111-1111-1111-1111-111111111119"),
                    Name = "Quadra de Beach Tennis Profissional",
                    EstablishmentId = centroTreinamento.Id,
                    SlotDurationMinutes = 60,
                    MinBookingSlots = 1,
                    MaxBookingSlots = 2,
                    OpeningTime = new TimeOnly(6, 0),
                    ClosingTime = new TimeOnly(22, 0),
                    PricePerSlot = 55.00m
                }, new List<Guid> { beachTennis.Id }));
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
