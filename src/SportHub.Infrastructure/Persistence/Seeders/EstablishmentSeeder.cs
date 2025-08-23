using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence.Seeders;

public class EstablishmentSeeder : BaseSeeder
{
    private readonly IEstablishmentsRepository _establishmentsRepository;
    private readonly IEstablishmentUsersRepository _establishmentUsersRepository;
    private readonly ISportsRepository _sportsRepository;

    public EstablishmentSeeder(
        IEstablishmentsRepository establishmentsRepository,
        IEstablishmentUsersRepository establishmentUsersRepository,
        ISportsRepository sportsRepository,
        ILogger<EstablishmentSeeder> logger) : base(logger)
    {
        _establishmentsRepository = establishmentsRepository;
        _establishmentUsersRepository = establishmentUsersRepository;
        _sportsRepository = sportsRepository;
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

            // Associar esportes
            if (estData.SportIds.Any())
            {
                var sports = await _sportsRepository.GetByIdsAsync(estData.SportIds, cancellationToken);
                estData.Establishment.Sports = sports;
                await _establishmentsRepository.UpdateAsync(estData.Establishment, cancellationToken);
                LogInfo($"Associated {sports.Count} sports to establishment: {estData.Establishment.Name}");
            }
        }

        LogInfo("Establishments seeding completed.");
    }

    private List<(Establishment Establishment, Guid OwnerId, List<Guid> SportIds)> GetTestEstablishments()
    {
        return new List<(Establishment, Guid, List<Guid>)>
        {
            (
                new Establishment
                {
                    Id = Guid.Parse("E1111111-1111-1111-1111-111111111111"),
                    Name = "SportHub Central Arena",
                    Description = "Complete sports complex in downtown with football, basketball and volleyball courts. Air-conditioned environment and state-of-the-art equipment.",
                    PhoneNumber = "+1 (555) 999-1111",
                    Email = "contact@sportcentralarena.com",
                    Website = "https://sportcentralarena.com",
                    ImageUrl = "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b?q=80&w=1000&auto=format&fit=crop",
                    Address = new Address(
                        "123 Palm Street",
                        "1000",
                        "Suite 101",
                        "Downtown",
                        "New York",
                        "NY",
                        "10001"
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
                    Name = "Premium Athletic Club",
                    Description = "Premium club focused on tennis and padel. Professional courts, complete locker rooms and leisure area.",
                    PhoneNumber = "+1 (555) 999-2222",
                    Email = "info@premiumathletic.com",
                    Website = "https://premiumathletic.com",
                    ImageUrl = "https://images.unsplash.com/photo-1622279457486-62dcc4a431d6?q=80&w=1000&auto=format&fit=crop",
                    Address = new Address(
                        "456 Broadway Avenue",
                        "2500",
                        null,
                        "Midtown",
                        "New York",
                        "NY",
                        "10036"
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
                    Name = "Futsal Mania Center",
                    Description = "Specialized in futsal with 4 official courts. Ideal place for casual games, tournaments and training sessions.",
                    PhoneNumber = "+1 (555) 999-3333",
                    Email = "reservations@futsalmania.com",
                    Website = "https://futsalmania.com",
                    ImageUrl = "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b?q=80&w=1000&auto=format&fit=crop",
                    Address = new Address(
                        "789 Sports Boulevard",
                        "500",
                        "Building A",
                        "Sports District",
                        "Los Angeles",
                        "CA",
                        "90210"
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
