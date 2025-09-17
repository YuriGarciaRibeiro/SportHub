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
            // Estabelecimentos originais
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
            ),
            
            // Novos estabelecimentos
            (
                new Establishment
                {
                    Id = Guid.Parse("E4444444-4444-4444-4444-444444444444"),
                    Name = "Arena Beach Tennis Sergipe",
                    Description = "Complexo de beach tennis com 6 quadras na areia. Local perfeito para jogar com vista para o mar na Orla de Atalaia.",
                    PhoneNumber = "+55 (79) 99999-4444",
                    Email = "contato@beachtennisaracaju.com.br",
                    Website = "https://beachtennisaracaju.com.br",
                    ImageUrl = "https://images.unsplash.com/photo-1551698618-1dfe5d97d256?q=80&w=1000&auto=format&fit=crop",
                    Address = new Address(
                        "Avenida Beira Mar",
                        "2100",
                        null,
                        "Atalaia",
                        "Aracaju",
                        "SE",
                        "49037-000",
                        geometryFactory.CreatePoint(new Coordinate(-37.0510, -10.9855))
                    )
                },
                Guid.Parse("44444444-4444-4444-4444-444444444444"), // Anna Brown
                new List<Guid> { Guid.Parse("A8888888-8888-8888-8888-888888888888") } // Beach Tennis
            ),
            (
                new Establishment
                {
                    Id = Guid.Parse("E5555555-5555-5555-5555-555555555555"),
                    Name = "Quadra do Bairro 13 de Julho",
                    Description = "Quadra comunitária de futebol society no coração do bairro 13 de Julho. Espaço acessível para todos os níveis.",
                    PhoneNumber = "+55 (79) 99999-5555",
                    Email = "quadra13julho@gmail.com",
                    Website = "https://quadra13julho.com.br",
                    ImageUrl = "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b?q=80&w=1000&auto=format&fit=crop",
                    Address = new Address(
                        "Rua Campos",
                        "845",
                        null,
                        "13 de Julho",
                        "Aracaju",
                        "SE",
                        "49020-380",
                        geometryFactory.CreatePoint(new Coordinate(-37.0612, -10.9198))
                    )
                },
                Guid.Parse("55555555-5555-5555-5555-555555555555"), // Peter Davis
                new List<Guid> { Guid.Parse("A1111111-1111-1111-1111-111111111111") } // Football
            ),
            (
                new Establishment
                {
                    Id = Guid.Parse("E6666666-6666-6666-6666-666666666666"),
                    Name = "Handebol Sergipe",
                    Description = "Centro especializado em handebol com duas quadras oficiais. Oferece treinos e campeonatos regulares.",
                    PhoneNumber = "+55 (79) 99999-6666",
                    Email = "contato@handebolse.com.br",
                    Website = "https://handebolse.com.br",
                    ImageUrl = "https://images.unsplash.com/photo-1578662996442-48f60103fc96?q=80&w=1000&auto=format&fit=crop",
                    Address = new Address(
                        "Rua Pacatuba",
                        "1200",
                        "Galpão B",
                        "Farolândia",
                        "Aracaju",
                        "SE",
                        "49030-320",
                        geometryFactory.CreatePoint(new Coordinate(-37.0890, -10.9445))
                    )
                },
                Guid.Parse("66666666-6666-6666-6666-666666666666"), // Lucy Miller
                new List<Guid> { Guid.Parse("A6666666-6666-6666-6666-666666666666") } // Handball
            ),
            (
                new Establishment
                {
                    Id = Guid.Parse("E7777777-7777-7777-7777-777777777777"),
                    Name = "Academia Multi Sport",
                    Description = "Academia completa com quadras de basquete, vôlei e futsal. Equipamentos modernos e professores qualificados.",
                    PhoneNumber = "+55 (79) 99999-7777",
                    Email = "academia@multisport.com.br",
                    Website = "https://multisport.com.br",
                    ImageUrl = "https://images.unsplash.com/photo-1546519638-68e109498ffc?q=80&w=1000&auto=format&fit=crop",
                    Address = new Address(
                        "Avenida Hermes Fontes",
                        "3400",
                        "Loja 12",
                        "Salgado Filho",
                        "Aracaju",
                        "SE",
                        "49020-490",
                        geometryFactory.CreatePoint(new Coordinate(-37.0701, -10.9301))
                    )
                },
                Guid.Parse("77777777-7777-7777-7777-777777777777"), // Robert Wilson
                new List<Guid> 
                { 
                    Guid.Parse("A2222222-2222-2222-2222-222222222222"), // Basketball
                    Guid.Parse("A3333333-3333-3333-3333-333333333333"), // Volleyball
                    Guid.Parse("A5555555-5555-5555-5555-555555555555")  // Futsal
                }
            ),
            (
                new Establishment
                {
                    Id = Guid.Parse("E8888888-8888-8888-8888-888888888888"),
                    Name = "Tennis Club Jardins",
                    Description = "Tradicional clube de tênis com 8 quadras no bairro Jardins. Ambiente familiar e aulas para todas as idades.",
                    PhoneNumber = "+55 (79) 99999-8888",
                    Email = "reservas@tennisclub.com.br",
                    Website = "https://tennisclub.com.br",
                    ImageUrl = "https://images.unsplash.com/photo-1622279457486-62dcc4a431d6?q=80&w=1000&auto=format&fit=crop",
                    Address = new Address(
                        "Rua Aracaju",
                        "567",
                        null,
                        "Jardins",
                        "Aracaju",
                        "SE",
                        "49026-010",
                        geometryFactory.CreatePoint(new Coordinate(-37.0701, -10.9123))
                    )
                },
                Guid.Parse("88888888-8888-8888-8888-888888888888"), // Emily Moore
                new List<Guid> { Guid.Parse("A4444444-4444-4444-4444-444444444444") } // Tennis
            ),
            (
                new Establishment
                {
                    Id = Guid.Parse("E9999999-9999-9999-9999-999999999999"),
                    Name = "Esporte Clube São Conrado",
                    Description = "Clube tradicional com múltiplas modalidades esportivas. Oferece futebol, basquete e vôlei em ambiente climatizado.",
                    PhoneNumber = "+55 (79) 99999-9999",
                    Email = "clube@saoconrado.com.br",
                    Website = "https://saoconrado.com.br",
                    ImageUrl = "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b?q=80&w=1000&auto=format&fit=crop",
                    Address = new Address(
                        "Rua São Conrado",
                        "1890",
                        null,
                        "São Conrado",
                        "Aracaju",
                        "SE",
                        "49042-120",
                        geometryFactory.CreatePoint(new Coordinate(-37.0523, -10.9634))
                    )
                },
                Guid.Parse("11111111-1111-1111-1111-111111111111"), // John Smith (pode ter múltiplos estabelecimentos)
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
                    Id = Guid.Parse("EA111111-1111-1111-1111-111111111111"),
                    Name = "Futsal Arena Inácio Barbosa",
                    Description = "Duas quadras oficiais de futsal no bairro Inácio Barbosa. Ideal para peladas e campeonatos locais.",
                    PhoneNumber = "+55 (79) 99988-1111",
                    Email = "futsal@inarenabarbosa.com.br",
                    Website = "https://futsalinarenabarbosa.com.br",
                    ImageUrl = "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b?q=80&w=1000&auto=format&fit=crop",
                    Address = new Address(
                        "Avenida João Alves Filho",
                        "2345",
                        null,
                        "Inácio Barbosa",
                        "Aracaju",
                        "SE",
                        "49040-530",
                        geometryFactory.CreatePoint(new Coordinate(-37.0423, -10.9812))
                    )
                },
                Guid.Parse("22222222-2222-2222-2222-222222222222"), // Mary Johnson
                new List<Guid> { Guid.Parse("A5555555-5555-5555-5555-555555555555") } // Futsal
            ),
            (
                new Establishment
                {
                    Id = Guid.Parse("EA222222-2222-2222-2222-222222222222"),
                    Name = "Padel Center Coroa do Meio",
                    Description = "Moderno centro de padel com 4 quadras panorâmicas. Localizado no Coroa do Meio com vista privilegiada.",
                    PhoneNumber = "+55 (79) 99988-2222",
                    Email = "reservas@padelcenter.com.br",
                    Website = "https://padelcenter.com.br",
                    ImageUrl = "https://images.unsplash.com/photo-1544966503-7cc5ac882d5f?q=80&w=1000&auto=format&fit=crop",
                    Address = new Address(
                        "Avenida Delmiro Gouveia",
                        "400",
                        "Shopping Coroa do Meio",
                        "Coroa do Meio",
                        "Aracaju",
                        "SE",
                        "49035-490",
                        geometryFactory.CreatePoint(new Coordinate(-37.0234, -10.9756))
                    )
                },
                Guid.Parse("33333333-3333-3333-3333-333333333333"), // Charles Williams
                new List<Guid> { Guid.Parse("A7777777-7777-7777-7777-777777777777") } // Padel
            ),
            (
                new Establishment
                {
                    Id = Guid.Parse("EA333333-3333-3333-3333-333333333333"),
                    Name = "Quadra Poliesportiva Bugio",
                    Description = "Quadra poliesportiva no bairro Bugio, atendendo futebol society e basquete. Ambiente familiar e preços acessíveis.",
                    PhoneNumber = "+55 (79) 99988-3333",
                    Email = "quadrabugio@gmail.com",
                    Website = "https://quadrabugio.com.br",
                    ImageUrl = "https://images.unsplash.com/photo-1546519638-68e109498ffc?q=80&w=1000&auto=format&fit=crop",
                    Address = new Address(
                        "Rua do Bugio",
                        "123",
                        null,
                        "Bugio",
                        "Aracaju",
                        "SE",
                        "49065-123",
                        geometryFactory.CreatePoint(new Coordinate(-37.0812, -10.9567))
                    )
                },
                Guid.Parse("44444444-4444-4444-4444-444444444444"), // Anna Brown
                new List<Guid> 
                { 
                    Guid.Parse("A1111111-1111-1111-1111-111111111111"), // Football
                    Guid.Parse("A2222222-2222-2222-2222-222222222222")  // Basketball
                }
            ),
            (
                new Establishment
                {
                    Id = Guid.Parse("EA444444-4444-4444-4444-444444444444"),
                    Name = "Vôlei de Praia Atalaia",
                    Description = "Quadras de vôlei de praia na areia da Praia de Atalaia. Ambiente descontraído para jogos casuais e torneios.",
                    PhoneNumber = "+55 (79) 99988-4444",
                    Email = "voleipraia@atalaia.com.br",
                    Website = "https://voleipraia-atalaia.com.br",
                    ImageUrl = "https://images.unsplash.com/photo-1612872087720-bb876e2e67d1?q=80&w=1000&auto=format&fit=crop",
                    Address = new Address(
                        "Passarela do Caranguejo",
                        "s/n",
                        "Orla de Atalaia",
                        "Atalaia",
                        "Aracaju",
                        "SE",
                        "49037-000",
                        geometryFactory.CreatePoint(new Coordinate(-37.0489, -10.9889))
                    )
                },
                Guid.Parse("55555555-5555-5555-5555-555555555555"), // Peter Davis
                new List<Guid> { Guid.Parse("A3333333-3333-3333-3333-333333333333") } // Volleyball
            ),
            (
                new Establishment
                {
                    Id = Guid.Parse("EA555555-5555-5555-5555-555555555555"),
                    Name = "Soccer Zone Grageru",
                    Description = "Complexo com 3 campos de futebol society no Grageru. Gramado sintético de alta qualidade e iluminação LED.",
                    PhoneNumber = "+55 (79) 99988-5555",
                    Email = "reservas@soccerzone.com.br",
                    Website = "https://soccerzone.com.br",
                    ImageUrl = "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b?q=80&w=1000&auto=format&fit=crop",
                    Address = new Address(
                        "Rua Itabaiana",
                        "890",
                        null,
                        "Grageru",
                        "Aracaju",
                        "SE",
                        "49025-530",
                        geometryFactory.CreatePoint(new Coordinate(-37.0656, -10.9234))
                    )
                },
                Guid.Parse("66666666-6666-6666-6666-666666666666"), // Lucy Miller
                new List<Guid> { Guid.Parse("A1111111-1111-1111-1111-111111111111") } // Football
            ),
            (
                new Establishment
                {
                    Id = Guid.Parse("EA666666-6666-6666-6666-666666666666"),
                    Name = "Centro de Tênis Santos Dumont",
                    Description = "Centro especializado em tênis com 6 quadras no bairro Santos Dumont. Aulas particulares e em grupo disponíveis.",
                    PhoneNumber = "+55 (79) 99988-6666",
                    Email = "tenis@santosdumont.com.br",
                    Website = "https://tenissantosdumont.com.br",
                    ImageUrl = "https://images.unsplash.com/photo-1622279457486-62dcc4a431d6?q=80&w=1000&auto=format&fit=crop",
                    Address = new Address(
                        "Rua Santos Dumont",
                        "1456",
                        null,
                        "Santos Dumont",
                        "Aracaju",
                        "SE",
                        "49087-260",
                        geometryFactory.CreatePoint(new Coordinate(-37.0789, -10.9456))
                    )
                },
                Guid.Parse("77777777-7777-7777-7777-777777777777"), // Robert Wilson
                new List<Guid> { Guid.Parse("A4444444-4444-4444-4444-444444444444") } // Tennis
            ),
            (
                new Establishment
                {
                    Id = Guid.Parse("EA777777-7777-7777-7777-777777777777"),
                    Name = "Arena Multi Sport Ponto Novo",
                    Description = "Arena moderna no Ponto Novo com quadras de basquete, vôlei e futsal. Equipamentos de última geração e vestiários completos.",
                    PhoneNumber = "+55 (79) 99988-7777",
                    Email = "arena@pontonovo.com.br",
                    Website = "https://arenamultisport.com.br",
                    ImageUrl = "https://images.unsplash.com/photo-1546519638-68e109498ffc?q=80&w=1000&auto=format&fit=crop",
                    Address = new Address(
                        "Avenida Presidente Tancredo Neves",
                        "2890",
                        "Loja 45",
                        "Ponto Novo",
                        "Aracaju",
                        "SE",
                        "49097-000",
                        geometryFactory.CreatePoint(new Coordinate(-37.0745, -10.9287))
                    )
                },
                Guid.Parse("88888888-8888-8888-8888-888888888888"), // Emily Moore
                new List<Guid> 
                { 
                    Guid.Parse("A2222222-2222-2222-2222-222222222222"), // Basketball
                    Guid.Parse("A3333333-3333-3333-3333-333333333333"), // Volleyball
                    Guid.Parse("A5555555-5555-5555-5555-555555555555")  // Futsal
                }
            ),
            (
                new Establishment
                {
                    Id = Guid.Parse("EA888888-8888-8888-8888-888888888888"),
                    Name = "Handebol Clube Jabotiana",
                    Description = "Clube especializado em handebol na Jabotiana. Duas quadras oficiais e programa de desenvolvimento de atletas.",
                    PhoneNumber = "+55 (79) 99988-8888",
                    Email = "handebol@jabotiana.com.br",
                    Website = "https://handebol-jabotiana.com.br",
                    ImageUrl = "https://images.unsplash.com/photo-1578662996442-48f60103fc96?q=80&w=1000&auto=format&fit=crop",
                    Address = new Address(
                        "Rua da Jabotiana",
                        "3456",
                        "Quadra 12",
                        "Jabotiana",
                        "Aracaju",
                        "SE",
                        "49095-000",
                        geometryFactory.CreatePoint(new Coordinate(-37.0834, -10.9123))
                    )
                },
                Guid.Parse("11111111-1111-1111-1111-111111111111"), // John Smith
                new List<Guid> { Guid.Parse("A6666666-6666-6666-6666-666666666666") } // Handball
            ),
            (
                new Establishment
                {
                    Id = Guid.Parse("EA999999-9999-9999-9999-999999999999"),
                    Name = "Beach Tennis Paradise",
                    Description = "Complexo de beach tennis na Orla de Atalaia com 8 quadras na areia. Vista para o mar e ambiente paradisíaco.",
                    PhoneNumber = "+55 (79) 99988-9999",
                    Email = "paradise@beachtennis.com.br",
                    Website = "https://beachtennisparadise.com.br",
                    ImageUrl = "https://images.unsplash.com/photo-1551698618-1dfe5d97d256?q=80&w=1000&auto=format&fit=crop",
                    Address = new Address(
                        "Avenida Oceânica",
                        "4567",
                        "Frente ao Mar",
                        "Atalaia",
                        "Aracaju",
                        "SE",
                        "49037-000",
                        geometryFactory.CreatePoint(new Coordinate(-37.0467, -10.9912))
                    )
                },
                Guid.Parse("22222222-2222-2222-2222-222222222222"), // Mary Johnson
                new List<Guid> { Guid.Parse("A8888888-8888-8888-8888-888888888888") } // Beach Tennis
            ),
            (
                new Establishment
                {
                    Id = Guid.Parse("EB111111-1111-1111-1111-111111111111"),
                    Name = "Quadra Popular Industrial",
                    Description = "Quadra comunitária no bairro Industrial para futebol society. Preços populares e ambiente familiar.",
                    PhoneNumber = "+55 (79) 99977-1111",
                    Email = "quadraindustrial@hotmail.com",
                    Website = "https://quadraindustrial.com.br",
                    ImageUrl = "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b?q=80&w=1000&auto=format&fit=crop",
                    Address = new Address(
                        "Rua Industrial",
                        "567",
                        null,
                        "Industrial",
                        "Aracaju",
                        "SE",
                        "49065-230",
                        geometryFactory.CreatePoint(new Coordinate(-37.0923, -10.9345))
                    )
                },
                Guid.Parse("33333333-3333-3333-3333-333333333333"), // Charles Williams
                new List<Guid> { Guid.Parse("A1111111-1111-1111-1111-111111111111") } // Football
            ),
            (
                new Establishment
                {
                    Id = Guid.Parse("EB222222-2222-2222-2222-222222222222"),
                    Name = "Padel Elite Luzia",
                    Description = "Centro de padel premium em Nossa Senhora de Lourdes com 6 quadras de vidro. Ambiente sofisticado e aulas profissionais.",
                    PhoneNumber = "+55 (79) 99977-2222",
                    Email = "elite@padel-luzia.com.br",
                    Website = "https://padelelite.com.br",
                    ImageUrl = "https://images.unsplash.com/photo-1544966503-7cc5ac882d5f?q=80&w=1000&auto=format&fit=crop",
                    Address = new Address(
                        "Avenida Beira Rio",
                        "1234",
                        "Torre A",
                        "Luzia",
                        "Aracaju",
                        "SE",
                        "49045-490",
                        geometryFactory.CreatePoint(new Coordinate(-37.0345, -10.9234))
                    )
                },
                Guid.Parse("44444444-4444-4444-4444-444444444444"), // Anna Brown
                new List<Guid> { Guid.Parse("A7777777-7777-7777-7777-777777777777") } // Padel
            ),
            (
                new Establishment
                {
                    Id = Guid.Parse("EB333333-3333-3333-3333-333333333333"),
                    Name = "Basquete Center Olaria",
                    Description = "Centro especializado em basquete na Olaria com 3 quadras oficiais. Programas para iniciantes e avançados.",
                    PhoneNumber = "+55 (79) 99977-3333",
                    Email = "basquete@olaria.com.br",
                    Website = "https://basquetecenter.com.br",
                    ImageUrl = "https://images.unsplash.com/photo-1546519638-68e109498ffc?q=80&w=1000&auto=format&fit=crop",
                    Address = new Address(
                        "Rua da Olaria",
                        "789",
                        "Galpão C",
                        "Olaria",
                        "Aracaju",
                        "SE",
                        "49027-390",
                        geometryFactory.CreatePoint(new Coordinate(-37.0612, -10.9445))
                    )
                },
                Guid.Parse("55555555-5555-5555-5555-555555555555"), // Peter Davis
                new List<Guid> { Guid.Parse("A2222222-2222-2222-2222-222222222222") } // Basketball
            ),
            (
                new Establishment
                {
                    Id = Guid.Parse("EB444444-4444-4444-4444-444444444444"),
                    Name = "Vôlei Total São José",
                    Description = "Academia de vôlei no São José com 4 quadras. Treinos para diferentes níveis e campeonatos regulares.",
                    PhoneNumber = "+55 (79) 99977-4444",
                    Email = "volei@saojose.com.br",
                    Website = "https://voleitotal.com.br",
                    ImageUrl = "https://images.unsplash.com/photo-1612872087720-bb876e2e67d1?q=80&w=1000&auto=format&fit=crop",
                    Address = new Address(
                        "Avenida São José",
                        "2345",
                        null,
                        "São José",
                        "Aracaju",
                        "SE",
                        "49015-270",
                        geometryFactory.CreatePoint(new Coordinate(-37.0567, -10.9178))
                    )
                },
                Guid.Parse("66666666-6666-6666-6666-666666666666"), // Lucy Miller
                new List<Guid> { Guid.Parse("A3333333-3333-3333-3333-333333333333") } // Volleyball
            ),
            (
                new Establishment
                {
                    Id = Guid.Parse("EB555555-5555-5555-5555-555555555555"),
                    Name = "Futsal Arena Cidade Nova",
                    Description = "Arena de futsal na Cidade Nova com 2 quadras oficiais. Ambiente moderno e equipamentos de qualidade.",
                    PhoneNumber = "+55 (79) 99977-5555",
                    Email = "arena@cidadenova.com.br",
                    Website = "https://futsalarena-cidadenova.com.br",
                    ImageUrl = "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b?q=80&w=1000&auto=format&fit=crop",
                    Address = new Address(
                        "Rua Nova Cidade",
                        "1567",
                        "Bloco A",
                        "Cidade Nova",
                        "Aracaju",
                        "SE",
                        "49073-020",
                        geometryFactory.CreatePoint(new Coordinate(-37.0789, -10.9567))
                    )
                },
                Guid.Parse("77777777-7777-7777-7777-777777777777"), // Robert Wilson
                new List<Guid> { Guid.Parse("A5555555-5555-5555-5555-555555555555") } // Futsal
            ),
            (
                new Establishment
                {
                    Id = Guid.Parse("EB666666-6666-6666-6666-666666666666"),
                    Name = "Tennis Academy Aeroporto",
                    Description = "Academia de tênis próxima ao aeroporto com 5 quadras. Programa de formação de atletas e aulas recreativas.",
                    PhoneNumber = "+55 (79) 99977-6666",
                    Email = "tennis@aeroporto.com.br",
                    Website = "https://tennisacademy.com.br",
                    ImageUrl = "https://images.unsplash.com/photo-1622279457486-62dcc4a431d6?q=80&w=1000&auto=format&fit=crop",
                    Address = new Address(
                        "Avenida Aeroporto",
                        "4567",
                        "Setor B",
                        "Aeroporto",
                        "Aracaju",
                        "SE",
                        "49037-535",
                        geometryFactory.CreatePoint(new Coordinate(-37.0234, -10.9612))
                    )
                },
                Guid.Parse("88888888-8888-8888-8888-888888888888"), // Emily Moore
                new List<Guid> { Guid.Parse("A4444444-4444-4444-4444-444444444444") } // Tennis
            ),
            (
                new Establishment
                {
                    Id = Guid.Parse("EB777777-7777-7777-7777-777777777777"),
                    Name = "Complexo Esportivo Zona Sul",
                    Description = "Grande complexo esportivo na zona sul com múltiplas modalidades. Futsal, basquete, vôlei e handebol em um só lugar.",
                    PhoneNumber = "+55 (79) 99977-7777",
                    Email = "complexo@zonasul.com.br",
                    Website = "https://complexo-zonasul.com.br",
                    ImageUrl = "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b?q=80&w=1000&auto=format&fit=crop",
                    Address = new Address(
                        "Avenida Zona Sul",
                        "5678",
                        "Complexo Esportivo",
                        "Zona de Expansão",
                        "Aracaju",
                        "SE",
                        "49037-690",
                        geometryFactory.CreatePoint(new Coordinate(-37.0123, -10.9823))
                    )
                },
                Guid.Parse("11111111-1111-1111-1111-111111111111"), // John Smith
                new List<Guid> 
                { 
                    Guid.Parse("A5555555-5555-5555-5555-555555555555"), // Futsal
                    Guid.Parse("A2222222-2222-2222-2222-222222222222"), // Basketball
                    Guid.Parse("A3333333-3333-3333-3333-333333333333"), // Volleyball
                    Guid.Parse("A6666666-6666-6666-6666-666666666666")  // Handball
                }
            ),
            (
                new Establishment
                {
                    Id = Guid.Parse("EB888888-8888-8888-8888-888888888888"),
                    Name = "Beach Sports Orla",
                    Description = "Centro de esportes de praia na Orla de Atalaia. Beach tennis, vôlei de praia e futebol de areia.",
                    PhoneNumber = "+55 (79) 99977-8888",
                    Email = "beach@orla.com.br",
                    Website = "https://beachsports-orla.com.br",
                    ImageUrl = "https://images.unsplash.com/photo-1551698618-1dfe5d97d256?q=80&w=1000&auto=format&fit=crop",
                    Address = new Address(
                        "Orla de Atalaia",
                        "s/n",
                        "Quiosque 15",
                        "Atalaia",
                        "Aracaju",
                        "SE",
                        "49037-000",
                        geometryFactory.CreatePoint(new Coordinate(-37.0456, -10.9934))
                    )
                },
                Guid.Parse("22222222-2222-2222-2222-222222222222"), // Mary Johnson
                new List<Guid> 
                { 
                    Guid.Parse("A8888888-8888-8888-8888-888888888888"), // Beach Tennis
                    Guid.Parse("A3333333-3333-3333-3333-333333333333"), // Volleyball
                    Guid.Parse("A1111111-1111-1111-1111-111111111111")  // Football
                }
            ),
            (
                new Establishment
                {
                    Id = Guid.Parse("EB999999-9999-9999-9999-999999999999"),
                    Name = "Arena Esportiva América",
                    Description = "Arena no bairro América com foco em futebol society e futsal. Gramado sintético e iluminação profissional.",
                    PhoneNumber = "+55 (79) 99977-9999",
                    Email = "arena@america.com.br",
                    Website = "https://arena-america.com.br",
                    ImageUrl = "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b?q=80&w=1000&auto=format&fit=crop",
                    Address = new Address(
                        "Rua América",
                        "890",
                        null,
                        "América",
                        "Aracaju",
                        "SE",
                        "49075-240",
                        geometryFactory.CreatePoint(new Coordinate(-37.0834, -10.9234))
                    )
                },
                Guid.Parse("33333333-3333-3333-3333-333333333333"), // Charles Williams
                new List<Guid> 
                { 
                    Guid.Parse("A1111111-1111-1111-1111-111111111111"), // Football
                    Guid.Parse("A5555555-5555-5555-5555-555555555555")  // Futsal
                }
            ),
            (
                new Establishment
                {
                    Id = Guid.Parse("EC111111-1111-1111-1111-111111111111"),
                    Name = "Centro de Treinamento Esportivo",
                    Description = "Centro completo para treinamento esportivo no Siqueira Campos. Todas as modalidades em um ambiente profissional.",
                    PhoneNumber = "+55 (79) 99966-1111",
                    Email = "centro@treinamento.com.br",
                    Website = "https://centrotreinamento.com.br",
                    ImageUrl = "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b?q=80&w=1000&auto=format&fit=crop",
                    Address = new Address(
                        "Avenida Siqueira Campos",
                        "3456",
                        "Centro de Treinamento",
                        "Siqueira Campos",
                        "Aracaju",
                        "SE",
                        "49075-490",
                        geometryFactory.CreatePoint(new Coordinate(-37.0778, -10.9378))
                    )
                },
                Guid.Parse("44444444-4444-4444-4444-444444444444"), // Anna Brown
                new List<Guid> 
                { 
                    Guid.Parse("A1111111-1111-1111-1111-111111111111"), // Football
                    Guid.Parse("A2222222-2222-2222-2222-222222222222"), // Basketball
                    Guid.Parse("A3333333-3333-3333-3333-333333333333"), // Volleyball
                    Guid.Parse("A4444444-4444-4444-4444-444444444444"), // Tennis
                    Guid.Parse("A5555555-5555-5555-5555-555555555555"), // Futsal
                    Guid.Parse("A6666666-6666-6666-6666-666666666666"), // Handball
                    Guid.Parse("A7777777-7777-7777-7777-777777777777"), // Padel
                    Guid.Parse("A8888888-8888-8888-8888-888888888888")  // Beach Tennis
                }
            )
        };
    }
}
