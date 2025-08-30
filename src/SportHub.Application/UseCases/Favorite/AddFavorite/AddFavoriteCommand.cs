using Application.CQRS;
using Domain.Enums;

namespace Application.UseCases.Favorite.AddFavorite;

public class AddFavoriteCommand : ICommand
{
    public Guid UserId { get; set; }
    public FavoriteType EntityType { get; set; }
    public Guid EntityId { get; set; }
}
