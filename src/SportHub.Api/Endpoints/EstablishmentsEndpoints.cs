using SportHub.Api.Endpoints.Establishments;

namespace SportHub.Api.Endpoints;

public static class EstablishmentsEndpoints
{
    public static void MapEstablishmentsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/establishments")
            .WithTags("Establishments");

        group.MapEstablishmentsCrudEndpoints();
        group.MapEstablishmentUsersEndpoints();
        group.MapEstablishmentCourtsEndpoints();
        group.MapEstablishmentSportsEndpoints();
        group.MapEstablishmentReservationsEndpoints();
    }
}
