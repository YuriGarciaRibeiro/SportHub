using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.CQRS;
using Application.UseCases.Reservations.GetMyReservations;

namespace Application.UseCases.Reservations.GetCourtReservations;

public class GetCourtReservationsHandler : IQueryHandler<GetCourtReservationsQuery, PagedResult<ReservationResponse>>
{
    private readonly IReservationRepository _reservationRepository;
    private readonly ICourtsRepository _courtsRepository;

    public GetCourtReservationsHandler(
        IReservationRepository reservationRepository,
        ICourtsRepository courtsRepository)
    {
        _reservationRepository = reservationRepository;
        _courtsRepository = courtsRepository;
    }

    public async Task<Result<PagedResult<ReservationResponse>>> Handle(GetCourtReservationsQuery request, CancellationToken cancellationToken)
    {
        var courtExists = await _courtsRepository.ExistsAsync(request.CourtId);
        if (!courtExists)
            return Result.Fail(new NotFound($"Quadra com ID {request.CourtId} não encontrada."));

        var filter = request.Filter;

        var paged = await _reservationRepository.GetPagedAsync(
            page: filter.Page ?? 1,
            pageSize: filter.PageSize ?? 10,
            courtId: request.CourtId,
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
                r.Status,
                r.Court.CancelationWindowHours ?? r.Court.Tenant.CancelationWindowHours,
                r.Court.LateCancellationFeePercent))],
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };

        return Result.Ok(result);
    }
}
