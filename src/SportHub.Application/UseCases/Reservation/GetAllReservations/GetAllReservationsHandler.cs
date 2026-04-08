using Application.Common.Interfaces;
using Application.Common.Models;
using Application.CQRS;
using Application.UseCases.Reservations.GetMyReservations;

namespace Application.UseCases.Reservations.GetAllReservations;

public class GetAllReservationsHandler : IQueryHandler<GetAllReservationsQuery, PagedResult<ReservationResponse>>
{
    private readonly IReservationRepository _reservationRepository;

    public GetAllReservationsHandler(IReservationRepository reservationRepository)
    {
        _reservationRepository = reservationRepository;
    }

    public async Task<Result<PagedResult<ReservationResponse>>> Handle(GetAllReservationsQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;

        var paged = await _reservationRepository.GetPagedAsync(
            page: filter.Page ?? 1,
            pageSize: filter.PageSize ?? 50,
            startDate: filter.StartDate,
            endDate: filter.EndDate);

        var result = new PagedResult<ReservationResponse>
        {
            Items = [..paged.Items.Select(r => new ReservationResponse(
                r.Id,
                r.CourtId,
                r.Court.Name,
                r.StartTimeUtc,
                r.EndTimeUtc,
                r.User.FullName,
                r.User.Email,
                r.CreatedByUser?.FullName,
                r.CreatedByUser?.Email,
                r.CreatedByUser?.Role.ToString(),
                r.TotalPrice,
                r.PricePerHour,
                r.IsPeakHours,
                r.NormalSlots,
                r.PeakSlots,
                r.NormalSubtotal,
                r.PeakSubtotal,
                r.NormalPricePerSlot,
                r.PeakPricePerSlot,
                r.Status))],
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };

        return Result.Ok(result);
    }
}
