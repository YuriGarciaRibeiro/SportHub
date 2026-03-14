using Application.CQRS;
using MediatR;

namespace Application.UseCases.Tenant.ActivateTenant;

public record ActivateTenantCommand(Guid Id) : ICommand<Unit>;
