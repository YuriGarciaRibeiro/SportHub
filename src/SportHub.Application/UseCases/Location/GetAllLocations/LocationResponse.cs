namespace Application.UseCases.Location.GetAllLocations;

public record LocationResponse(
    Guid Id,
    string Name,
    AddressResponse? Address,
    string? Phone,
    List<DailyScheduleResponse> BusinessHours,
    bool IsDefault,
    string? InstagramUrl,
    string? FacebookUrl,
    string? WhatsappNumber);

public record AddressResponse(
    string? Street,
    string? Number,
    string? Complement,
    string? Neighborhood,
    string? City,
    string? State,
    string? ZipCode);

public record DailyScheduleResponse(
    DayOfWeek DayOfWeek,
    bool IsOpen,
    string? OpenTime,
    string? CloseTime);
