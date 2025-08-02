using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.CQRS;

namespace Application.UseCases.Court.UpdateCourt;

public class UpdateCourtHandler : ICommandHandler<UpdateCourtCommand, UpdateCourtResponse>
{
    private readonly ICourtsRepository _courtRepository;
    private readonly ISportsRepository _sportRepository;

    public UpdateCourtHandler(ICourtsRepository courtRepository, ISportsRepository sportRepository)
    {
        _courtRepository = courtRepository;
        _sportRepository = sportRepository;
    }

    public async Task<Result<UpdateCourtResponse>> Handle(UpdateCourtCommand request, CancellationToken cancellationToken)
    {
        var court = await _courtRepository.GetByIdAsync(request.Id);
        if (court == null)
        {
            return Result.Fail(new NotFound($"Court with ID '{request.Id}' not found."));
        }

        if (court.EstablishmentId != request.EstablishmentId)
        {
            return Result.Fail(new BadRequest("Court does not belong to the specified establishment."));
        }

        var updatedCourt = new Domain.Entities.Court
        {
            Id = court.Id,
            Name = request.Request.Name ?? court.Name,
            SlotDurationMinutes = request.Request.SlotDurationMinutes ?? court.SlotDurationMinutes,
            MinBookingSlots = request.Request.MinBookingSlots ?? court.MinBookingSlots,
            MaxBookingSlots = request.Request.MaxBookingSlots ?? court.MaxBookingSlots,
            OpeningTime = request.Request.OpeningTime ?? court.OpeningTime,
            ClosingTime = request.Request.ClosingTime ?? court.ClosingTime,
            TimeZone = request.Request.TimeZone ?? court.TimeZone
        };

        if (request.Request.SportIds != null)
        {
            var sports = await _sportRepository.GetByIdsAsync(request.Request.SportIds);
            if (sports.Count != request.Request.SportIds.Count())
            {
                return Result.Fail(new BadRequest("One or more specified sports do not exist."));
            }

            updatedCourt.Sports = sports;
        }

        await _courtRepository.UpdateAsync(updatedCourt);

        return Result.Ok(new UpdateCourtResponse
        {
            Id = updatedCourt.Id,
            Name = updatedCourt.Name,
            SlotDurationMinutes = updatedCourt.SlotDurationMinutes,
            MinBookingSlots = updatedCourt.MinBookingSlots,
            MaxBookingSlots = updatedCourt.MaxBookingSlots,
            OpeningTime = updatedCourt.OpeningTime,
            ClosingTime = updatedCourt.ClosingTime,
            TimeZone = updatedCourt.TimeZone,
            CreatedAtUtc = updatedCourt.CreatedAtUtc,
            Sports = updatedCourt.Sports
        });
    }
}