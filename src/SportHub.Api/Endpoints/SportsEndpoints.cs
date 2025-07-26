using Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace SportHub.Api.Endpoints;

public static class SportsEndpoints
{
    public static void MapSportsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/sports")
            .WithTags("Sports")
            .WithOpenApi();

        group.MapGet("/", GetAllSports)
            .WithName("GetAllSports")
            .WithSummary("Get all sports")
            .Produces<IEnumerable<SportDto>>(200);
    }

    private static async Task<IResult> GetAllSports(ISportsRepository sportsRepository)
    {
        var sports = await sportsRepository.GetAllAsync();
        var sportDtos = sports.Select(s => new SportDto
        {
            Id = s.Id,
            Name = s.Name,
            Description = s.Description,
            ImageUrl = s.ImageUrl
        });

        return Results.Ok(sportDtos);
    }
}

public class SportDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string ImageUrl { get; set; } = null!;
}
