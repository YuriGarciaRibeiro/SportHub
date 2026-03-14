using Application.CQRS;
using MediatR;

namespace Application.UseCases.Tenant.SuspendTenant;

public record SuspendTenantCommand(Guid Id) : ICommand<Unit>;
