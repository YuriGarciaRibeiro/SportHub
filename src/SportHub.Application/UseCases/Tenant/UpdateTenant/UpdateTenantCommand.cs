using Application.CQRS;
using MediatR;

namespace Application.UseCases.Tenant.UpdateTenant;

public record UpdateTenantCommand(Guid Id, string? LogoUrl, string? PrimaryColor) : ICommand<Unit>;
