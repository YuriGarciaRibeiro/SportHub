using Application.Common.Errors;
using Application.Common.Interfaces.Establishments;
using Application.CQRS;

namespace Application.UseCases.Establishments.GetNearbyEstablishments;

public class GetNearbyEstablishmentsHandler : IQueryHandler<GetNearbyEstablishmentsQuery, GetNearbyEstablishmentsResponse>
{
    private readonly IEstablishmentsRepository _establishmentsRepository;

    public GetNearbyEstablishmentsHandler(IEstablishmentsRepository establishmentsRepository)
    {
        _establishmentsRepository = establishmentsRepository;
    }

    public async Task<Result<GetNearbyEstablishmentsResponse>> Handle(GetNearbyEstablishmentsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var establishments = await _establishmentsRepository.GetNearbyEstablishmentsAsync(
                request.Latitude, 
                request.Longitude, 
                request.RadiusKm, 
                cancellationToken);

            return new GetNearbyEstablishmentsResponse(establishments);
        }
        catch (Exception ex)
        {
            return new InternalServerError($"Failed to search nearby establishments: {ex.Message}");
        }
    }
}
