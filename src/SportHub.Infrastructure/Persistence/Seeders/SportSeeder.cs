using Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence.Seeders;

public class SportSeeder : BaseSeeder
{
    private readonly ISportsRepository _sportsRepository;

    public SportSeeder(ISportsRepository sportsRepository, ILogger<SportSeeder> logger) : base(logger)
    {
        _sportsRepository = sportsRepository;
    }

    public override int Order => 2; // Segundo a ser executado

    public override async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        LogInfo("Starting sports seeding...");

        var sports = GetDefaultSports();

        foreach (var sport in sports)
        {
            var exists = await _sportsRepository.ExistsByNameAsync(sport.Name, cancellationToken);
            if (!exists)
            {
                LogInfo($"Creating sport: {sport.Name}");
                await _sportsRepository.AddAsync(sport, cancellationToken);
            }
            else
            {
                LogInfo($"Sport already exists: {sport.Name}");
            }
        }

        LogInfo("Sports seeding completed.");
    }

    private List<Sport> GetDefaultSports()
    {
        return new List<Sport>
        {
            new Sport
            {
                Id = Guid.Parse("A1111111-1111-1111-1111-111111111111"),
                Name = "Football",
                Description = "Sport played on a grass field with two teams of 11 players each, where the objective is to score goals in the opposing goal.",
                ImageUrl = "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b?q=80&w=1000&auto=format&fit=crop"
            },
            new Sport
            {
                Id = Guid.Parse("A2222222-2222-2222-2222-222222222222"),
                Name = "Basketball",
                Description = "Sport played on an indoor court by two teams of 5 players each, where the objective is to shoot the ball into the opposing basket.",
                ImageUrl = "https://images.unsplash.com/photo-1546519638-68e109498ffc?q=80&w=1000&auto=format&fit=crop"
            },
            new Sport
            {
                Id = Guid.Parse("A3333333-3333-3333-3333-333333333333"),
                Name = "Volleyball",
                Description = "Sport played on a court divided by a net, with two teams of 6 players each, where the objective is to make the ball touch the opponent's court floor.",
                ImageUrl = "https://images.unsplash.com/photo-1612872087720-bb876e2e67d1?q=80&w=1000&auto=format&fit=crop"
            },
            new Sport
            {
                Id = Guid.Parse("A4444444-4444-4444-4444-444444444444"),
                Name = "Tennis",
                Description = "Racquet sport played individually or in pairs, where the goal is to hit the ball to the opponent's side of the court.",
                ImageUrl = "https://images.unsplash.com/photo-1622279457486-62dcc4a431d6?q=80&w=1000&auto=format&fit=crop"
            },
            new Sport
            {
                Id = Guid.Parse("A5555555-5555-5555-5555-555555555555"),
                Name = "Futsal",
                Description = "A variant of football played indoors, with two teams of 5 players each, including a goalkeeper.",
                ImageUrl = "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b?q=80&w=1000&auto=format&fit=crop"
            },
            new Sport
            {
                Id = Guid.Parse("A6666666-6666-6666-6666-666666666666"),
                Name = "Handball",
                Description = "Team sport played with the hands, where two teams of 7 players each try to score goals in the opposing goal.",
                ImageUrl = "https://images.unsplash.com/photo-1578662996442-48f60103fc96?q=80&w=1000&auto=format&fit=crop"
            },
            new Sport
            {
                Id = Guid.Parse("A7777777-7777-7777-7777-777777777777"),
                Name = "Padel",
                Description = "Racquet sport played in doubles on an enclosed court, combining elements of tennis and squash.",
                ImageUrl = "https://images.unsplash.com/photo-1544966503-7cc5ac882d5f?q=80&w=1000&auto=format&fit=crop"
            },
            new Sport
            {
                Id = Guid.Parse("A8888888-8888-8888-8888-888888888888"),
                Name = "Beach Tennis",
                Description = "Racquet sport played on sand without letting the ball bounce, combining elements of tennis, beach volleyball and badminton.",
                ImageUrl = "https://images.unsplash.com/photo-1551698618-1dfe5d97d256?q=80&w=1000&auto=format&fit=crop"
            }
        };
    }
}
