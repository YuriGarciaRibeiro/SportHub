

using Application.Common.Enums;
using Application.CQRS;

namespace Application.UseCases.Court.GetAvailability;

public class GetAvailabilityHandler : IQueryHandler<GetAvailabilityQuery, GetAvailabilityResponse>
{
    private readonly IReservationService reservationService;
    private readonly ICacheService cacheService;

    public GetAvailabilityHandler(IReservationService reservationService, ICacheService cacheService)
    {
        this.cacheService = cacheService;
        this.reservationService = reservationService;
    }

    public async Task<Result<GetAvailabilityResponse>> Handle(GetAvailabilityQuery request, CancellationToken cancellationToken)
    {

        var availableSlots = await reservationService.GetAvailableSlotsAsync(request.CourtId, request.Date, cancellationToken);
        if (availableSlots.IsFailed)
        {
            return Result.Fail(availableSlots.Errors);
        }

        var response = new GetAvailabilityResponse
        {
            Date = request.Date,
            AvailableSlotsUtc = availableSlots.Value
        };
        
        return Result.Ok(response);
    }
}
