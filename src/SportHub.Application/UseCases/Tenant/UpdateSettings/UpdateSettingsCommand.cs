using Application.CQRS;
using MediatR;

namespace Application.UseCases.Tenant.UpdateSettings;

public record UpdateSettingsCommand(
    string Name,
    string? LogoUrl,
    string? PrimaryColor,
    string? Tagline,
    string? Description,
    string? InstagramUrl,
    string? FacebookUrl,
    string? WhatsappNumber,
    int? CancelationWindowHours,
    bool PeakHoursEnabled) : ICommand<Unit>;
