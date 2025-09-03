using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace Infrastructure.Persistence.Seeders;

public class EstablishmentSeeder : BaseSeeder
{
    private readonly IEstablishmentsRepository _establishmentsRepository;
    private readonly IEstablishmentUsersRepository _establishmentUsersRepository;
    private readonly ISportsRepository _sportsRepository;
    private readonly ApplicationDbContext _dbContext;

    public EstablishmentSeeder(
        IEstablishmentsRepository establishmentsRepository,
        IEstablishmentUsersRepository establishmentUsersRepository,
        ISportsRepository sportsRepository,
        ApplicationDbContext dbContext,
        ILogger<EstablishmentSeeder> logger) : base(logger)
    {
        _establishmentsRepository = establishmentsRepository;
        _establishmentUsersRepository = establishmentUsersRepository;
        _sportsRepository = sportsRepository;
        _dbContext = dbContext;
    }

    public override int Order => 3; // Terceiro a ser executado (após Users e Sports)

    public override async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        LogInfo("Starting establishments seeding...");

        var establishments = GetTestEstablishments();
        
        foreach (var estData in establishments)
        {
            var existing = await _establishmentsRepository.GetByIdAsync(estData.Establishment.Id, cancellationToken);
            if (existing != null)
            {
                LogInfo($"Establishment already exists: {estData.Establishment.Name}");
                continue;
            }

            // Criar estabelecimento
            await _establishmentsRepository.AddAsync(estData.Establishment, cancellationToken);
            LogInfo($"Created establishment: {estData.Establishment.Name}");

            // Associar usuário como proprietário
            var establishmentUser = new EstablishmentUser
            {
                EstablishmentId = estData.Establishment.Id,
                UserId = estData.OwnerId,
                Role = EstablishmentRole.Owner
            };

            await _establishmentUsersRepository.AddAsync(establishmentUser, cancellationToken);
            LogInfo($"Assigned owner to establishment: {estData.Establishment.Name}");

            // Associar esportes usando DbContext diretamente para evitar conflitos de tracking
            if (estData.SportIds.Any())
            {
                // Use raw SQL or direct DbContext operations to avoid tracking conflicts
                var existingSports = await _dbContext.Sports
                    .Where(s => estData.SportIds.Contains(s.Id))
                    .ToListAsync(cancellationToken);

                var establishment = await _dbContext.Establishments
                    .Include(e => e.Sports)
                    .FirstOrDefaultAsync(e => e.Id == estData.Establishment.Id, cancellationToken);

                if (establishment != null)
                {
                    // Clear existing sports (if any) and add new ones
                    establishment.Sports.Clear();
                    foreach (var sport in existingSports)
                    {
                        establishment.Sports.Add(sport);
                    }

                    await _dbContext.SaveChangesAsync(cancellationToken);
                    LogInfo($"Associated {existingSports.Count} sports to establishment: {estData.Establishment.Name}");
                }
            }
        }

        LogInfo("Establishments seeding completed.");
    }

    private List<(Establishment Establishment, Guid OwnerId, List<Guid> SportIds)> GetTestEstablishments()
    {
        var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        
        return new List<(Establishment, Guid, List<Guid>)>
        {
            (
                new Establishment
                {
                    Id = Guid.Parse("E1111111-1111-1111-1111-111111111111"),
                    Name = "SportHub Arena Central",
                    Description = "Complexo esportivo completo no centro de Aracaju com quadras de futebol, basquete e vôlei. Ambiente climatizado e equipamentos de última geração.",
                    PhoneNumber = "+55 (79) 99999-1111",
                    Email = "contato@sporthubcentral.com.br",
                    Website = "https://sporthubcentral.com.br",
                    ImageUrl = "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b?q=80&w=1000&auto=format&fit=crop",
                    Address = new Address(
                        "Rua João Pessoa",
                        "450",
                        "Sala 101",
                        "Centro",
                        "Aracaju",
                        "SE",
                        "49010-230",
                        geometryFactory.CreatePoint(new Coordinate(-37.0548, -10.9065)) // Longitude, Latitude
                    )
                },
                Guid.Parse("11111111-1111-1111-1111-111111111111"), // John Smith
                new List<Guid> 
                { 
                    Guid.Parse("A1111111-1111-1111-1111-111111111111"), // Football
                    Guid.Parse("A2222222-2222-2222-2222-222222222222"), // Basketball
                    Guid.Parse("A3333333-3333-3333-3333-333333333333")  // Volleyball
                }
            ),
            (
                new Establishment
                {
                    Id = Guid.Parse("E2222222-2222-2222-2222-222222222222"),
                    Name = "Club Premium Atalaia",
                    Description = "Clube premium focado em tênis e padel na Praia de Atalaia. Quadras profissionais, vestiários completos e área de lazer.",
                    PhoneNumber = "+55 (79) 99999-2222",
                    Email = "info@clubpremiumatalaia.com.br",
                    Website = "https://clubpremiumatalaia.com.br",
                    ImageUrl = "https://images.unsplash.com/photo-1622279457486-62dcc4a431d6?q=80&w=1000&auto=format&fit=crop",
                    Address = new Address(
                        "Avenida Santos Dumont",
                        "1500",
                        null,
                        "Atalaia",
                        "Aracaju",
                        "SE",
                        "49037-470",
                        geometryFactory.CreatePoint(new Coordinate(-37.0493, -10.9838)) // Longitude, Latitude
                    )
                },
                Guid.Parse("22222222-2222-2222-2222-222222222222"), // Mary Johnson
                new List<Guid> 
                { 
                    Guid.Parse("A4444444-4444-4444-4444-444444444444"), // Tennis
                    Guid.Parse("A7777777-7777-7777-7777-777777777777")  // Padel
                }
            ),
            (
                new Establishment
                {
                    Id = Guid.Parse("E3333333-3333-3333-3333-333333333333"),
                    Name = "Centro de Futsal Sergipe",
                    Description = "Especializado em futsal com 4 quadras oficiais no bairro Ponto Novo. Local ideal para jogos casuais, torneios e treinos.",
                    PhoneNumber = "+55 (79) 99999-3333",
                    Email = "reservas@futsalsergipe.com.br",
                    Website = "https://futsalsergipe.com.br",
                    ImageUrl = "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b?q=80&w=1000&auto=format&fit=crop",
                    Address = new Address(
                        "Rua Desembargador Maynard",
                        "789",
                        "Galpão A",
                        "Ponto Novo",
                        "Aracaju",
                        "SE",
                        "49097-000",
                        geometryFactory.CreatePoint(new Coordinate(-37.0731, -10.9212)) // Longitude, Latitude
                    )
                },
                Guid.Parse("33333333-3333-3333-3333-333333333333"), // Charles Williams
                new List<Guid> 
                { 
                    Guid.Parse("A5555555-5555-5555-5555-555555555555") // Futsal
                }
            )
        };
    }
}
