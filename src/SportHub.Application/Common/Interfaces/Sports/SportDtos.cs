namespace Application.Common.Interfaces.Sports;

public record SportSummaryDto(
    Guid Id,
    string Name,
    string Description
);

public record SportBulkDto(
    Guid Id,
    string Name,
    string Description
);

public record SportCompleteDto(
    Guid Id,
    string Name,
    string Description,
    IEnumerable<EstablishmentSummaryDto> Establishments
);

public record EstablishmentSummaryDto(
    Guid Id,
    string Name,
    string Description
);
