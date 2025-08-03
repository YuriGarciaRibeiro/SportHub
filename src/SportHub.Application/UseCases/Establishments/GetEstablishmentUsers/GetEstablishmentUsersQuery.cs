using Application.CQRS;

namespace Application.UseCases.Establishments.GetEstablishmentUsers;

public class GetEstablishmentUsersQuery : IQuery<GetEstablishmentUsersResponse>
{
    public Guid EstablishmentId { get; set; }

}
