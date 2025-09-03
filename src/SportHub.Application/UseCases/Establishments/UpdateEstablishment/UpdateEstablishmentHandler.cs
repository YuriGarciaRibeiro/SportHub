using Application.Common.Errors;
using Application.CQRS;
using Domain.ValueObjects;

namespace Application.UseCases.Establishments.UpdateEstablishment;

public class UpdateEstablishmentHandler : ICommandHandler<UpdateEstablishmentCommand, UpdateEstablishmentResponse>
{
    private readonly IEstablishmentService _establishmentService;

    public UpdateEstablishmentHandler(IEstablishmentService establishmentService)
    {
        _establishmentService = establishmentService;
    }

    public async Task<Result<UpdateEstablishmentResponse>> Handle(UpdateEstablishmentCommand request, CancellationToken cancellationToken)
    {
        var establishment = await _establishmentService.GetByIdAsync(request.Id, ct: cancellationToken);
        if (establishment == null)
        {
            return Result.Fail(new NotFound("Establishment not found."));
        }

        var establishmentRequest = request.Request;

        establishment.Name = establishmentRequest.Name ?? establishment.Name;
        establishment.Description = establishmentRequest.Description ?? establishment.Description;
        establishment.ImageUrl = establishmentRequest.ImageUrl ?? establishment.ImageUrl;
        if (establishmentRequest.Address is not null)
        {
            var addr = establishment.Address;
            establishment.Address = new Address(
                establishmentRequest.Address.Street ?? addr.Street,
                establishmentRequest.Address.Number ?? addr.Number,
                establishmentRequest.Address.Complement ?? addr.Complement,
                establishmentRequest.Address.Neighborhood ?? addr.Neighborhood,
                establishmentRequest.Address.City ?? addr.City,
                establishmentRequest.Address.State ?? addr.State,
                establishmentRequest.Address.ZipCode ?? addr.ZipCode
            );
        }

        await _establishmentService.UpdateAsync(establishment, ct: cancellationToken);

        return Result.Ok(new UpdateEstablishmentResponse(
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
                establishment.Address.ZipCode,
                establishment.Address.Latitude,
                establishment.Address.Longitude
            ),
            establishment.ImageUrl
        ));
    }
}
