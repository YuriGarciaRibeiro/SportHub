

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

        var slotsResult = await reservationService.GetSlotsAsync(request.CourtId, request.Date);
        if (slotsResult.IsFailed)
            return Result.Fail(slotsResult.Errors);

        var response = new GetAvailabilityResponse
        {
            Date = request.Date,
            AvailableSlotsUtc = slotsResult.Value.Where(s => s.IsAvailable).Select(s => s.SlotUtc).ToList(),
            SlotsUtc = slotsResult.Value.Select(s => new SlotInfo { StartUtc = s.SlotUtc, IsAvailable = s.IsAvailable }).ToList(),
        };

        await cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(30), cancellationToken);
        return Result.Ok(response);
    }
}
