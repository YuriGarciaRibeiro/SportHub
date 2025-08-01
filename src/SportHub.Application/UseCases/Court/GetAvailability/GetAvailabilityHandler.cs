

using Application.Common.Enums;
using Application.Common.Interfaces;
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
        var cacheKey = cacheService.GenerateCacheKey(CacheKeyPrefix.GetAvailability, request.CourtId, request.Date.ToString("yyyy-MM-dd"));
        var cachedResponse = await cacheService.GetAsync<GetAvailabilityResponse>(cacheKey, cancellationToken);
        if (cachedResponse != null)
        {
            return Result.Ok(cachedResponse);
        }

        var availableSlots = await reservationService.GetAvailableSlotsAsync(request.CourtId, request.Date);
        if (availableSlots.IsFailed)
        {
            return Result.Fail(availableSlots.Errors);
        }

        var response = new GetAvailabilityResponse
        {
            Date = request.Date,
            AvailableSlotsUtc = availableSlots.Value
        };

        await cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(30), cancellationToken);
        return Result.Ok(response);
    }
}
