namespace Application.UseCases.Establishments.GetEstablishmentUsers;

public class GetEstablishmentUsersResponse
{
    public Guid EstablishmentId { get; set; }
    public List<EstablishmentUserResponse> Users { get; set; } = new List<EstablishmentUserResponse>();

}
public class EstablishmentUserResponse
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = null!;
    public string Role { get; set; } = null!;
}