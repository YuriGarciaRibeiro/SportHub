using Application.CQRS;
using Domain.Enums;

namespace Application.UseCases.Establishments.UpdateEstablishmentUserRole;

public class UpdateEstablishmentUserRoleCommand : ICommand
{
    public Guid EstablishmentId { get; set; }
    public Guid UserId { get; set; }
    public UpdateEstablishmentUserRoleRequest Request { get; set; } = new UpdateEstablishmentUserRoleRequest();
}

public class UpdateEstablishmentUserRoleRequest
{
    public EstablishmentRole Role { get; set; } = EstablishmentRole.Staff;
}