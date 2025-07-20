using Domain.ValueObjects;

namespace Domain.Entities;

public class Establishment
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Website { get; set; } = null!;
    public string ImageUrl { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public Address Address { get; set; } = null!;

    public ICollection<EstablishmentUser> Users { get; set; } = new List<EstablishmentUser>();
}