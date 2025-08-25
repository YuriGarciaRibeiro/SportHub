using Application.Common.Errors;
using Application.CQRS;

namespace Application.UseCases.Court.UpdateCourt;

public class UpdateCourtHandler : ICommandHandler<UpdateCourtCommand, UpdateCourtResponse>
{
    private readonly ICourtService _courtService;
    private readonly ISportService _sportService;

    public UpdateCourtHandler(ICourtService courtService, ISportService sportService)
    {
        _courtService = courtService;
        _sportService = sportService;
    }

    public async Task<Result<UpdateCourtResponse>> Handle(UpdateCourtCommand request, CancellationToken cancellationToken)
    {
        var court = await _courtService.GetByIdAsync(request.Id, ct: cancellationToken);
        if (court == null)
        {
            return Result.Fail(new NotFound($"Court with ID '{request.Id}' not found."));
        }

        if (court.EstablishmentId != request.EstablishmentId)
        {
            return Result.Fail(new BadRequest("Court does not belong to the specified establishment."));
        }

        court.Name = request.Request.Name ?? court.Name;
        court.SlotDurationMinutes = request.Request.SlotDurationMinutes ?? court.SlotDurationMinutes;
        court.MinBookingSlots = request.Request.MinBookingSlots ?? court.MinBookingSlots;
        court.MaxBookingSlots = request.Request.MaxBookingSlots ?? court.MaxBookingSlots;
        court.OpeningTime = request.Request.OpeningTime ?? court.OpeningTime;
        court.ClosingTime = request.Request.ClosingTime ?? court.ClosingTime;
        court.TimeZone = request.Request.TimeZone ?? court.TimeZone;
        

        if (request.Request.SportIds != null)
        {
            var sports = await _sportService.GetByIdsAsync(request.Request.SportIds, cancellationToken);
            if (sports.Count != request.Request.SportIds.Count())
            {
                return Result.Fail(new BadRequest("One or more specified sports do not exist."));
            }

            court.Sports = sports;
        }

        await _courtService.UpdateAsync(court, cancellationToken);

        return Result.Ok(new UpdateCourtResponse
        {
            Id = court.Id,
            Name = court.Name,
            SlotDurationMinutes = court.SlotDurationMinutes,
            MinBookingSlots = court.MinBookingSlots,
            MaxBookingSlots = court.MaxBookingSlots,
            OpeningTime = court.OpeningTime,
            ClosingTime = court.ClosingTime,
            TimeZone = court.TimeZone,
            Sports = court.Sports
        });
    }
}