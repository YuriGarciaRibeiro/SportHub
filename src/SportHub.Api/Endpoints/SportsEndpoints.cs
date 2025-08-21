using Application.UseCases.Sports.GetAllSports;
using MediatR;
using WebApi.Extensions.ResultExtensions;

namespace SportHub.Api.Endpoints;

public static class SportsEndpoints
{
    public static void MapSportsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/sports")
            .WithTags("Sports")
            .WithOpenApi();

        group.MapGet("/", async (
            ISender sender) =>
        {
            var result = await sender.Send(new GetAllSportsQuery());

            return result.ToIResult(StatusCodes.Status200OK);
        })
            .WithName("GetAllSports")
            .WithSummary("Get all sports")
            .Produces<IEnumerable<SportDto>>(200);

    }
}


