using Application.Common.Enums;
using Application.CQRS;

namespace Application.UseCases.Establishments.GetEstablishments;

public record GetEstablishmentsQuery(
    Guid? ownerId,
    bool? isAvailable,
    double? latitude,
    double? longitude,
    EstablishmentOrderBy? orderBy = null,
    SortDirection? sortDirection = null,
    int page = 1,
    int pageSize = 10
) : IQuery<GetEstablishmentsResponse>;