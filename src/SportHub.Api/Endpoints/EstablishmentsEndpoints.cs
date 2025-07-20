using MediatR;
using WebApi.Extensions.ResultExtensions;

public static class EstablishmentsEndpoints
{
    public static void MapEstablishmentsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/establishments")
            .WithTags("Establishments");



        group.MapPost("/", async (
            CreateEstablishmentCommand command,
            ISender sender) =>
        {
            var result = await sender.Send(command);

            return result.ToIResult();
        })
        .WithName("CreateEstablishment")
        .WithSummary("Create a new establishment")
        .WithDescription("Creates a new establishment with the provided details.")
        .RequireAuthorization();

        group.MapGet("/{id:guid}", async (
            Guid id,
            ISender sender) =>
        {
            var result = await sender.Send(new GetEstablishmentQuery(id));

            return result.ToIResult();
        })
        .WithName("GetEstablishment")
        .WithSummary("Get an establishment by ID")
        .WithDescription("Retrieves an establishment by its ID.");
        
    }
}
