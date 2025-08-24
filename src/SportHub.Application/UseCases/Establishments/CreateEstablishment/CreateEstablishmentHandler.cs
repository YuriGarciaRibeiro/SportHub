using Application.Common.Interfaces;
using Application.CQRS;
using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.Establishments.CreateEstablishment;

public class CreateEstablishmentHandler : ICommandHandler<CreateEstablishmentCommand, string>
{
    private readonly IEstablishmentService _establishmentService;
    private readonly IEstablishmentUserService _establishmentUserService;
    private readonly IUserService _userService;
    private readonly ICurrentUserService _currentUser;
    private readonly ISportService _sportService;
    private readonly ILogger<CreateEstablishmentHandler> _logger;

    public CreateEstablishmentHandler(
        IEstablishmentService establishmentService,
        IEstablishmentUserService establishmentUserService,
        IUserService userService,
        ICurrentUserService currentUser,
        ILogger<CreateEstablishmentHandler> logger,
        ISportService sportService)
    {
        _logger = logger;
        _currentUser = currentUser;
        _establishmentService = establishmentService;
        _establishmentUserService = establishmentUserService;
        _sportService = sportService;
        _userService = userService;
    }

    public async Task<Result<string>> Handle(CreateEstablishmentCommand request, CancellationToken cancellationToken)
    {
        var sports = await _sportService.GetByIdsAsync(request.Sports, cancellationToken);

        var address = new Address(
            request.Street,
            request.Number,
            request.Complement,
            request.Neighborhood,
            request.City,
            request.State,
            request.ZipCode);

        var establishment = new Establishment
        {
            Name = request.Name,
            Description = request.Description,
            PhoneNumber = request.PhoneNumber,
            Email = request.Email,
            Website = request.Website,
            ImageUrl = request.ImageUrl,
            Address = address,
            Sports = sports.ToList()
        };


        var newEstablishmentUser = new Domain.Entities.EstablishmentUser
        {
            EstablishmentId = establishment.Id,
            UserId = _currentUser.UserId,
            Role = EstablishmentRole.Owner,
        };

        await _establishmentService.CreateAsync(establishment, cancellationToken);
        await _establishmentUserService.CreateAsync(newEstablishmentUser, ct: cancellationToken);

        var currentUser = await _userService.GetUserByIdAsync(_currentUser.UserId, cancellationToken);
        if (currentUser.Value.Role == UserRole.User)
        {
            await _userService.AddRoleToUserAsync(_currentUser.UserId, UserRole.EstablishmentMember, cancellationToken);
        }

        return Result.Ok(establishment.Id.ToString());
    }
}



    
