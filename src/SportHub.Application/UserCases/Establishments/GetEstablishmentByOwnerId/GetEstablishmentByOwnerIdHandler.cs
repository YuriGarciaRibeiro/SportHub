using Application.Common.Interfaces;
using Application.Common.Errors;
using Application.CQRS;
using Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Application.UserCases.Establishments.GetEstablishmentByOwnerId;

public class GetEstablishmentByOwnerIdHandler : IQueryHandler<GetEstablishmentByOwnerIdQuery, GetEstablishmentsByOwnerIdResponse>
{
    private readonly IEstablishmentService _establishmentService;
    private readonly IUserService _userService;
    private readonly ILogger<GetEstablishmentByOwnerIdHandler> _logger;
    private readonly ICurrentUserService _currentUserService;

    public GetEstablishmentByOwnerIdHandler(
        IEstablishmentService establishmentService,
        IUserService userService,
        ILogger<GetEstablishmentByOwnerIdHandler> logger,
        ICurrentUserService currentUserService)
    {
        _establishmentService = establishmentService;
        _userService = userService;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<Result<GetEstablishmentsByOwnerIdResponse>> Handle(GetEstablishmentByOwnerIdQuery request, CancellationToken cancellationToken)
    {
        var ownerId = _currentUserService.UserId;

        var establishments = await _establishmentService.GetEstablishmentsByOwnerIdAsync(ownerId);

        if (establishments.IsFailed)
            return Result.Fail(establishments.Errors);

        if (!establishments.Value.Any())
        {
            return Result.Ok(new GetEstablishmentsByOwnerIdResponse(new List<EstablishmentResponse>()));
        }

        var userIds = establishments.Value
            .SelectMany(e => e.Users)
            .Select(eu => eu.UserId)
            .Distinct()
            .ToList();

        var userResults = await _userService.GetUsersByIdsAsync(userIds);
        if (userResults.IsFailed)
            return Result.Fail(userResults.Errors);

        var usersDict = userResults.Value.ToDictionary(u => u.Id);

        var establishmentResponses = establishments.Value.Select(establishment =>
        {
            var establishmentUserResponses = establishment.Users
                .Select(eu =>
                {
                    var user = usersDict[eu.UserId];
                    return new EstablishmentUserResponse(
                        UserId: user.Id,
                        FirstName: user.FirstName,
                        LastName: user.LastName,
                        Email: user.Email,
                        Role: eu.Role.ToString()
                    );
                })
                .ToList();

            return new EstablishmentResponse(
                establishment.Id,
                establishment.Name,
                establishment.Description,
                new AddressResponse(
                    establishment.Address.Street,
                    establishment.Address.Number,
                    establishment.Address.Complement,
                    establishment.Address.Neighborhood,
                    establishment.Address.City,
                    establishment.Address.State,
                    establishment.Address.ZipCode),
                establishment.ImageUrl,
                establishmentUserResponses
            );
        }).ToList();

        return Result.Ok(new GetEstablishmentsByOwnerIdResponse(establishmentResponses));
    }
}
