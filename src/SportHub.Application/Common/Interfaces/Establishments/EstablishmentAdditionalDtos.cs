using Domain.Enums;

namespace Application.Common.Interfaces.Establishments;

public record EstablishmentWithDetailsDto(
    Guid Id,
    string Name,
    string Description,
    AddressDto Address,
    string? ImageUrl,
    IEnumerable<SportDto> Sports,
    IEnumerable<EstablishmentUserDto> Users,
    IEnumerable<CourtDto> Courts
);

public record EstablishmentWithAddressDto(
    Guid Id,
    string Name,
    string Description,
    AddressDto Address,
    string? ImageUrl
);

public record EstablishmentUserSummaryDto(
    Guid UserId,
    string FirstName,
    string LastName,
    string Email,
    string FullName,
    EstablishmentRole Role
);

public record EstablishmentSummaryDto(
    Guid Id,
    string Name,
    string Description
);
