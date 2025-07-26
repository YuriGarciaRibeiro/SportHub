using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class SportSeeder
{
    private readonly ISportsRepository _sportsRepository;
    private readonly ILogger<SportSeeder> _logger;

    public SportSeeder(ISportsRepository sportsRepository, ILogger<SportSeeder> logger)
    {
        _sportsRepository = sportsRepository;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        var sports = GetDefaultSports();

        foreach (var sport in sports)
        {
            var exists = await _sportsRepository.ExistsAsync(sport.Name);
            if (!exists)
            {
                _logger.LogInformation($"Seeding sport: {sport.Name}");
                await _sportsRepository.CreateAsync(sport);
            }
        }
    }

    private List<Sport> GetDefaultSports()
    {
        return new List<Sport>
        {
            new Sport
            {
                Id = Guid.NewGuid(),
                Name = "Football",
                Description = "Sport played on a grass field with two teams of 11 players each, where the objective is to score goals in the opposing goal.",
                ImageUrl = "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b?q=80&w=1000&auto=format&fit=crop"
            },
            new Sport
            {
                Id = Guid.NewGuid(),
                Name = "Basketball",
                Description = "Sport played on an indoor court by two teams of 5 players each, where the objective is to shoot the ball into the opposing basket.",
                ImageUrl = "https://images.unsplash.com/photo-1546519638-68e109498ffc?q=80&w=1000&auto=format&fit=crop"
            },
            new Sport
            {
                Id = Guid.NewGuid(),
                Name = "Volleyball",
                Description = "Sport played on a court divided by a net, with two teams of 6 players each, where the objective is to make the ball touch the opponent’s court floor.",
                ImageUrl = "https://images.unsplash.com/photo-1612872087720-bb876e2e67d1?q=80&w=1000&auto=format&fit=crop"
            },
            new Sport
            {
                Id = Guid.NewGuid(),
                Name = "Tennis",
                Description = "Racquet sport played individually or in pairs, where the goal is to hit the ball to the opponent’s side of the court.",
                ImageUrl = "https://images.unsplash.com/photo-1622279457486-62dcc4a431d6?q=80&w=1000&auto=format&fit=crop"
            },
            new Sport
            {
                Id = Guid.NewGuid(),
                Name = "Futsal",
                Description = "A variant of football played indoors, with two teams of 5 players each, including a goalkeeper.",
                ImageUrl = "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b?q=80&w=1000&auto=format&fit=crop"
            },
            new Sport
            {
                Id = Guid.NewGuid(),
                Name = "Handball",
                Description = "Team sport played with the hands, where two teams of 7 players each try to score goals in the opposing goal.",
                ImageUrl = "https://images.unsplash.com/photo-1578662996442-48f60103fc96?q=80&w=1000&auto=format&fit=crop"
            },
            new Sport
            {
                Id = Guid.NewGuid(),
                Name = "Beach Tennis",
                Description = "Sport played on sand, combining elements of tennis, beach volleyball, and badminton.",
                ImageUrl = "https://images.unsplash.com/photo-1551698618-1dfe5d97d256?q=80&w=1000&auto=format&fit=crop"
            },
            new Sport
            {
                Id = Guid.NewGuid(),
                Name = "Squash",
                Description = "Racquet sport played in an enclosed court with four walls, where players alternate hitting the ball against the front wall.",
                ImageUrl = "https://images.unsplash.com/photo-1551698618-1dfe5d97d256?q=80&w=1000&auto=format&fit=crop"
            }
        };
    }
}

