using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.CQRS;
using Application.UseCases.Location.CreateLocation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.Location.UpdateLocation;

public class UpdateLocationHandler : ICommandHandler<UpdateLocationCommand, Unit>
{
    private readonly ILocationsRepository _locationsRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateLocationHandler> _logger;

    public UpdateLocationHandler(
        ILocationsRepository locationsRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateLocationHandler> logger)
    {
        _locationsRepository = locationsRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Unit>> Handle(UpdateLocationCommand request, CancellationToken ct)
    {
        var location = await _locationsRepository.GetByIdAsync(request.Id);
        if (location is null)
            return Result.Fail(new NotFound($"Unidade com ID {request.Id} não encontrada."));

        _logger.LogInformation("Atualizando unidade: {LocationId} - {LocationName}", request.Id, request.Location.Name);

        location.Name = request.Location.Name;
        location.Address = MapAddress(request.Location.Address);
        location.Phone = request.Location.Phone;
        location.BusinessHours = MapBusinessHours(request.Location.BusinessHours);
        location.IsDefault = request.Location.IsDefault;
        location.InstagramUrl = request.Location.InstagramUrl;
        location.FacebookUrl = request.Location.FacebookUrl;
        location.WhatsappNumber = request.Location.WhatsappNumber;

        await _locationsRepository.UpdateAsync(location);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Ok(Unit.Value);
    }

    private static SportHub.Domain.Common.Address? MapAddress(AddressRequest? req) =>
        req is null ? null : new SportHub.Domain.Common.Address
        {
            Street = req.Street,
            Number = req.Number,
            Complement = req.Complement,
            Neighborhood = req.Neighborhood,
            City = req.City,
            State = req.State,
            ZipCode = req.ZipCode
        };

    private static List<SportHub.Domain.Common.DailySchedule> MapBusinessHours(List<DailyScheduleRequest> hours) =>
        hours.Select(h => new SportHub.Domain.Common.DailySchedule
        {
            DayOfWeek = h.DayOfWeek,
            IsOpen = h.IsOpen,
            OpenTime = h.IsOpen ? h.OpenTime : null,
            CloseTime = h.IsOpen ? h.CloseTime : null,
        }).ToList();
}
