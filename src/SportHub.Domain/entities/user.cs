
namespace Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;

    public ICollection<EstablishmentUser> Establishments { get; set; } = new List<EstablishmentUser>();
}
