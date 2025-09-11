using Domain.Common;
using Domain.ValueObjects;

namespace Domain.Entities;

public class Establishment : AuditEntity, IEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Website { get; set; } = null!;
    public string ImageUrl { get; set; } = null!;
    public TimeOnly OpeningTime { get; set; }
    public TimeOnly ClosingTime { get; set; }
    public string TimeZone { get; set; } = "America/Maceio";

    public Address Address { get; set; } = null!;

    public ICollection<EstablishmentUser> Users { get; set; } = new List<EstablishmentUser>();
    public ICollection<Court> Courts { get; set; } = new List<Court>();
    public ICollection<Sport> Sports { get; set; } = new List<Sport>();
}
