using Domain.Enums;

namespace Application.Common.Interfaces.Favorites;

public class FavoriteDto
{
    public Guid Id { get; set; }
    public FavoriteType EntityType { get; set; }
    public Guid EntityId { get; set; }
    public string? EntityName { get; set; } 
}