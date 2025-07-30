

using Application.Common.Interfaces;
using Application.CQRS;

namespace Application.UseCases.Court.GetAvailability;

public class GetAvailabilityHandler : IQueryHandler<GetAvailabilityQuery, GetAvailabilityResponse>
{
    private readonly IReservationService reservationService;

    public GetAvailabilityHandler(IReservationService reservationService)
    {
        this.reservationService = reservationService;
    }

    public async Task<Result<GetAvailabilityResponse>> Handle(GetAvailabilityQuery request, CancellationToken cancellationToken)
    {
        var availableSlots = await reservationService.GetAvailableSlotsAsync(request.CourtId, request.Date);
        if (availableSlots.IsFailed)
        {
            return Result.Fail(availableSlots.Errors);
        }

        return Result.Ok(new GetAvailabilityResponse
        {
            Date = request.Date,
            AvailableSlotsUtc = availableSlots.Value
        });
    }
}
