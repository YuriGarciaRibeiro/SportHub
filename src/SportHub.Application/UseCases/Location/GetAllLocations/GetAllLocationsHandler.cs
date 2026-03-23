using Application.Common.Interfaces;
using Application.CQRS;

namespace Application.UseCases.Location.GetAllLocations;

public class GetAllLocationsHandler : IQueryHandler<GetAllLocationsQuery, List<LocationResponse>>
{
    private readonly ILocationsRepository _locationsRepository;

    public GetAllLocationsHandler(ILocationsRepository locationsRepository)
    {
        _locationsRepository = locationsRepository;
    }

    public async Task<Result<List<LocationResponse>>> Handle(GetAllLocationsQuery request, CancellationToken ct)
    {
        var locations = await _locationsRepository.GetAllAsync();

        var response = locations.Select(l => new LocationResponse(
            l.Id,
            l.Name,
            l.Address is null ? null : new AddressResponse(
                l.Address.Street,
                l.Address.Number,
                l.Address.Complement,
                l.Address.Neighborhood,
                l.Address.City,
                l.Address.State,
                l.Address.ZipCode),
            l.Phone,
            l.BusinessHours.Select(h => new DailyScheduleResponse(h.DayOfWeek, h.IsOpen, h.OpenTime, h.CloseTime)).ToList(),
            l.IsDefault,
            l.InstagramUrl,
            l.FacebookUrl,
            l.WhatsappNumber
        )).ToList();

        return Result.Ok(response);
    }
}
