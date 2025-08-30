using Application.CQRS;
using Domain.Enums;

namespace Application.UseCases.Favorite.RemoveFavorite;

public class RemoveFavoriteCommand : ICommand
{
    public Guid UserId { get; set; }
    public FavoriteType EntityType { get; set; }
    public Guid EntityId { get; set; }
}
