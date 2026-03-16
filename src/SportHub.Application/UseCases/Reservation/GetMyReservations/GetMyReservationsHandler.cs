using Application.Common.Interfaces;
using Application.Common.Models;
using Application.CQRS;

namespace Application.UseCases.Reservations.GetMyReservations;

public class GetMyReservationsHandler : IQueryHandler<GetMyReservationsQuery, PagedResult<ReservationResponse>>
{
    private readonly IReservationRepository _reservationRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetMyReservationsHandler(
        IReservationRepository reservationRepository,
        ICurrentUserService currentUserService)
    {
        _reservationRepository = reservationRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PagedResult<ReservationResponse>>> Handle(GetMyReservationsQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;

        var paged = await _reservationRepository.GetPagedAsync(
            page: filter.Page ?? 1,
            pageSize: filter.PageSize ?? 10,
            userId: _currentUserService.UserId,
            startDate: filter.StartDate,
            endDate: filter.EndDate);

        var result = new PagedResult<ReservationResponse>
        {
            Items = [..paged.Items.Select(r => new ReservationResponse(
                r.Id,
                r.CourtId,
                r.Court.Name,
                r.StartTimeUtc,
                r.EndTimeUtc))],
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };

        return Result.Ok(result);
    }
}
