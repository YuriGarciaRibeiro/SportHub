using Application.Common.Interfaces;
using Application.Common.Errors;
using Application.CQRS;

namespace Application.UseCases.Establishments.GetEstablishmentById;

public class GetEstablishmentByIdHandler : IQueryHandler<GetEstablishmentByIdQuery, GetEstablishmentByIdResponse>
{
    private readonly IEstablishmentsRepository _establishmentRepository;
    private readonly IUserService _userService;

    public GetEstablishmentByIdHandler(IEstablishmentsRepository establishmentRepository, IUserService userService)
    {
        _establishmentRepository = establishmentRepository;
        _userService = userService;
    }

    public async Task<Result<GetEstablishmentByIdResponse>> Handle(GetEstablishmentByIdQuery request, CancellationToken cancellationToken)
    {
        var establishment = await _establishmentRepository.GetByIdAsync(request.Id);
        if (establishment == null)
        {
            return Result.Fail(new NotFound($"Establishment with ID '{request.Id}' not found."));
        }

        var userTasks = establishment.Users.Select(async e =>
        {
            var user = await _userService.GetUserByIdAsync(e.UserId);
            return new EstablishmentUserResponse(
                UserId: e.UserId,
                FirstName: user.FirstName,
                LastName: user.LastName,
                Email: user.Email,
                Role: e.Role.ToString()
            );
        });

        var users = await Task.WhenAll(userTasks);

        var response = new GetEstablishmentByIdResponse(
            Id: establishment.Id,
            Name: establishment.Name,
            Description: establishment.Description,
            Address: new AddressResponse(
                Street: establishment.Address.Street,
                Number: establishment.Address.Number,
                City: establishment.Address.City,
                State: establishment.Address.State,
                ZipCode: establishment.Address.ZipCode,
                Complement: establishment.Address.Complement,
                Neighborhood: establishment.Address.Neighborhood
            ),
            Users: users,
            ImageUrl: establishment.ImageUrl
        );

        return Result.Ok(response);
    }
}
