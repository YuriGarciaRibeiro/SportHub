using Application.Common.Interfaces.Favorites;
using Application.CQRS;
using Domain.Enums;

namespace Application.UseCases.Favorite.RemoveFavorite;

public class RemoveFavoriteHandler : ICommandHandler<RemoveFavoriteCommand>
{
    private readonly IFavoriteService _favoriteService;

    public RemoveFavoriteHandler(IFavoriteService favoriteService)
    {
        _favoriteService = favoriteService;
    }

    public async Task<Result> Handle(RemoveFavoriteCommand request, CancellationToken cancellationToken)
    {
        return await _favoriteService.RemoveFavoriteAsync(request.UserId, request.EntityType, request.EntityId, cancellationToken);
    }
}
