using Application.CQRS;

namespace Application.UseCases.Establishments.GetEstablishments;

public record GetEstablishmentsQuery(
    Guid? ownerId,
    bool? isAvailable,
    int page = 1,
    int pageSize = 10
) : IQuery<GetEstablishmentsResponse>;