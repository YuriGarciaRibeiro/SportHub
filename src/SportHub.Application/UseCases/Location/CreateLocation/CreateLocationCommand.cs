using Application.CQRS;

namespace Application.UseCases.Location.CreateLocation;

public class CreateLocationCommand : ICommand<Guid>
{
    public CreateLocationCommand(LocationRequest location)
    {
        Location = location;
    }

    public LocationRequest Location { get; set; } = new LocationRequest();
}

public class LocationRequest
{
    public string Name { get; set; } = null!;
    public AddressRequest? Address { get; set; }
    public string? Phone { get; set; }
    public List<DailyScheduleRequest> BusinessHours { get; set; } = [];
    public bool IsDefault { get; set; }
    public string? InstagramUrl { get; set; }
    public string? FacebookUrl { get; set; }
    public string? WhatsappNumber { get; set; }
}

public class AddressRequest
{
    public string? Street { get; set; }
    public string? Number { get; set; }
    public string? Complement { get; set; }
    public string? Neighborhood { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
}

public class DailyScheduleRequest
{
    /// <summary>0 = Domingo, 1 = Segunda ... 6 = Sábado</summary>
    public DayOfWeek DayOfWeek { get; set; }
    public bool IsOpen { get; set; }
    public string? OpenTime { get; set; }
    public string? CloseTime { get; set; }
}
