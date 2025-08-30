using Application.Common.Interfaces.Favorites;
using Application.CQRS;
using Domain.Enums;

namespace Application.UseCases.Favorite.AddFavorite;

public class AddFavoriteHandler : ICommandHandler<AddFavoriteCommand>
{
    private readonly IFavoriteService _favoriteService;

    public AddFavoriteHandler(IFavoriteService favoriteService)
    {
        _favoriteService = favoriteService;
    }

    public async Task<Result> Handle(AddFavoriteCommand request, CancellationToken cancellationToken)
    {
        return await _favoriteService.AddFavoriteAsync(request.UserId, request.EntityType, request.EntityId, cancellationToken);
    }
}
