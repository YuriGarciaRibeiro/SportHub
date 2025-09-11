using Application.Common.Errors;
using Application.Common.Interfaces.Favorites;
using Application.Common.Interfaces.Security;
using Application.CQRS;
using Domain.Enums;

namespace Application.UseCases.Establishments.GetEstablishmentById;

public class GetEstablishmentByIdHandler : IQueryHandler<GetEstablishmentByIdQuery, GetEstablishmentByIdResponse>
{
    private readonly IEstablishmentService _establishmentService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFavoriteService _favoriteService;

    public GetEstablishmentByIdHandler(
        IEstablishmentService establishmentService, 
        ICurrentUserService currentUserService,
        IFavoriteService favoriteService)
    {
        _establishmentService = establishmentService;
        _currentUserService = currentUserService;
        _favoriteService = favoriteService;
    }

    public async Task<Result<GetEstablishmentByIdResponse>> Handle(GetEstablishmentByIdQuery request, CancellationToken cancellationToken)
    {
        var establishment = await _establishmentService.GetByIdCompleteAsync(request.Id, request.Latitude, request.Longitude, ct: cancellationToken);
        if (establishment == null)
        {
            return Result.Fail(new NotFound($"Establishment with ID '{request.Id}' not found."));
        }

        var users = establishment.Users.Select(u => new EstablishmentUserResponse(
            UserId: u.UserId,
            FirstName: u.Name.Split(' ').FirstOrDefault() ?? "",
            LastName: string.Join(" ", u.Name.Split(' ').Skip(1)),
            Email: u.Email,
            Role: u.Role.ToString()
        ));

        var courts = establishment.Courts.Select(c => new CourtResponse(
            Id: c.Id,
            Name: c.Name,
            MinBookingSlots: c.MinBookingSlots,
            MaxBookingSlots: c.MaxBookingSlots,
            SlotDurationMinutes: c.SlotDurationMinutes,
            TimeZone: establishment.TimeZone,
            PricePerSlot: c.PricePerSlot,
            Sports: c.Sports.Select(s => new SportResponse(
                Id: s.Id,
                Name: s.Name,
                Description: s.Description
            ))
        ));

        bool? isFavorite = null;
        if (_currentUserService.UserId != Guid.Empty)
        {
            var favorites = await _favoriteService.GetFavoritesAsync(
                _currentUserService.UserId, 
                FavoriteType.Establishment, 
                cancellationToken);
            isFavorite = favorites.Any(f => f.EntityId == establishment.Id);
        }

        var response = new GetEstablishmentByIdResponse(
            Id: establishment.Id,
            Name: establishment.Name,
            Description: establishment.Description,
            PhoneNumber: establishment.PhoneNumber,
            Email: establishment.Email,
            Website: establishment.Website,
            ImageUrl: establishment.ImageUrl,
            OpeningTime: establishment.OpeningTime,
            ClosingTime: establishment.ClosingTime,
            Address: new AddressResponse(
                Street: establishment.Address.Street,
                Number: establishment.Address.Number,
                City: establishment.Address.City,
                State: establishment.Address.State,
                ZipCode: establishment.Address.ZipCode,
                Complement: establishment.Address.Complement,
                Neighborhood: establishment.Address.Neighborhood,
                Latitude: establishment.Address.Latitude,
                Longitude: establishment.Address.Longitude
            ),
            IsFavorite: isFavorite,
            Users: users,
            Courts: courts,
            Sports: establishment.Sports.Select(s => new SportResponse(
                Id: s.Id,
                Name: s.Name,
                Description: s.Description
            )),
            DistanceKm: establishment.DistanceKm,
            AverageRating: establishment.AverageRating,
            Evaluations: establishment.Evaluations.Select(e => new EvaluationResponse(
                Id: e.Id,
                UserId: e.UserId,
                UserName: e.UserName,
                Rating: e.Rating,
                Comment: e.Comment,
                CreatedAt: e.CreatedAt
            ))
        );

        return Result.Ok(response);
    }
}
