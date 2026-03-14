using Application.CQRS;
using MediatR;

namespace Application.UseCases.Tenant.UpdateSettings;

public record UpdateSettingsCommand(string Name, string? LogoUrl, string? PrimaryColor) : ICommand<Unit>;
