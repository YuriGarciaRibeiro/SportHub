using Application.Common.Errors;
using Application.CQRS;

namespace Application.UseCases.Establishments.GetEstablishmentUsers;

public class GetEstablishmentUsersHandler : IQueryHandler<GetEstablishmentUsersQuery, GetEstablishmentUsersResponse>
{
    private readonly IEstablishmentService _establishmentService;

    public GetEstablishmentUsersHandler(IEstablishmentService establishmentService)
    {
        _establishmentService = establishmentService;
    }

    public async Task<Result<GetEstablishmentUsersResponse>> Handle(GetEstablishmentUsersQuery request, CancellationToken cancellationToken)
    {
        var establishment = await _establishmentService.GetByIdNoTrackingAsync(request.EstablishmentId, cancellationToken);
        if (establishment == null)
        {
            return Result.Fail(new NotFound("Establishment not found."));
        }

        var users = await _establishmentService.GetUsersByEstablishmentIdAsync(request.EstablishmentId, cancellationToken);

        var response = new GetEstablishmentUsersResponse
        {
            EstablishmentId = request.EstablishmentId,
            Users = users.Select(u => new EstablishmentUserResponse
            {
                UserId = u.UserId,
                UserName = u.FullName,
                Role = u.Role.ToString()
            }).ToList()
        };

        return Result.Ok(response);
    }
}
