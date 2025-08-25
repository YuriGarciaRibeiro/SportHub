using Application.CQRS;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.Establishments.GetEstablishments;

public class GetEstablishmentsQueryHandler : IQueryHandler<GetEstablishmentsQuery, GetEstablishmentsResponse>
{
    private readonly IEstablishmentService _establishmentService;
    private readonly ILogger<GetEstablishmentsQueryHandler> _logger;

    public GetEstablishmentsQueryHandler(IEstablishmentService establishmentService, ILogger<GetEstablishmentsQueryHandler> logger)
    {
        _establishmentService = establishmentService;
        _logger = logger;
    }

    public async Task<Result<GetEstablishmentsResponse>> Handle(GetEstablishmentsQuery request, CancellationToken cancellationToken)
{
    var (items, totalCount) = await _establishmentService.GetFilteredAsync(request, cancellationToken);

    var response = new GetEstablishmentsResponse
    {
        Items = items,
        TotalCount = totalCount,
        Page = request.page,
        PageSize = request.pageSize
    };

    return Result.Ok(response);
}
}
