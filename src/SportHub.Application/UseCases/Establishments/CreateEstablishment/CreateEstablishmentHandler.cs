using Application.Common.Interfaces;
using Application.CQRS;
using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.Establishments.CreateEstablishment;

public class CreateEstablishmentHandler : ICommandHandler<CreateEstablishmentCommand, string>
{
    private readonly IEstablishmentsRepository _establishmentRepository;
    private readonly IEstablishmentUsersRepository _establishmentUsersRepository;
    private readonly IUserService _userService;
    private readonly ICurrentUserService _currentUser;
    private readonly ISportsRepository _sportsRepository;
    private readonly ILogger<CreateEstablishmentHandler> _logger;

    public CreateEstablishmentHandler(
        IEstablishmentsRepository establishmentRepository,
        IEstablishmentUsersRepository establishmentUsersRepository,
        IUserService userService,
        ICurrentUserService currentUser,
        ILogger<CreateEstablishmentHandler> logger,
        ISportsRepository sportsRepository)
    {
        _logger = logger;
        _currentUser = currentUser;
        _establishmentRepository = establishmentRepository;
        _establishmentUsersRepository = establishmentUsersRepository;
        _sportsRepository = sportsRepository;
        _userService = userService;
    }

    public async Task<Result<string>> Handle(CreateEstablishmentCommand request, CancellationToken cancellationToken)
    {
        var sports = await _sportsRepository.GetSportsByIdsAsync(request.Sports);

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
            Id = Guid.NewGuid(),
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

        await _establishmentRepository.AddAsync(establishment);
        await _establishmentUsersRepository.AddAsync(newEstablishmentUser);

        var currentUser = await _userService.GetUserByIdAsync(_currentUser.UserId);
        if (currentUser.Value.Role == UserRole.User)
        {
            await _userService.AddRoleToUserAsync(_currentUser.UserId, UserRole.EstablishmentMember);
        }

        return Result.Ok(establishment.Id.ToString());
    }
}



    
