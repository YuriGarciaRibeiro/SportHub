using Application.Common.Interfaces;
using Application.CQRS;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.Location.CreateLocation;

public class CreateLocationHandler : ICommandHandler<CreateLocationCommand, Guid>
{
    private readonly ILocationsRepository _locationsRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateLocationHandler> _logger;

    public CreateLocationHandler(
        ILocationsRepository locationsRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateLocationHandler> logger)
    {
        _locationsRepository = locationsRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateLocationCommand request, CancellationToken ct)
    {
        _logger.LogInformation("Creating location: {LocationName}", request.Location.Name);

        var location = new Domain.Entities.Location
        {
            Name = request.Location.Name,
            Address = MapAddress(request.Location.Address),
            Phone = request.Location.Phone,
            BusinessHours = MapBusinessHours(request.Location.BusinessHours),
            IsDefault = request.Location.IsDefault,
            InstagramUrl = request.Location.InstagramUrl,
            FacebookUrl = request.Location.FacebookUrl,
            WhatsappNumber = request.Location.WhatsappNumber
        };

        await _locationsRepository.AddAsync(location);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Ok(location.Id);
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
