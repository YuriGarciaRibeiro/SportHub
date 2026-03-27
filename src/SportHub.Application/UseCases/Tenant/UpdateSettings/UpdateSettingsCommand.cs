using Application.CQRS;
using MediatR;

namespace Application.UseCases.Tenant.UpdateSettings;

public record UpdateSettingsCommand(
    string Name,
    string? LogoUrl,
    string? PrimaryColor,
    string? Tagline,
    string? InstagramUrl,
    string? FacebookUrl,
    string? WhatsappNumber,
    bool PeakHoursEnabled) : ICommand<Unit>;
