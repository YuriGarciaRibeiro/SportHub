using Application.Common.Interfaces;
using Application.CQRS;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.Establishments.GetEstablishments;

public class GetEstablishmentsQueryHandler : IQueryHandler<GetEstablishmentsQuery, GetEstablishmentsResponse>
{
    private readonly IEstablishmentsRepository _repository;
    private readonly ILogger<GetEstablishmentsQueryHandler> _logger;

    public GetEstablishmentsQueryHandler(IEstablishmentsRepository repository, ILogger<GetEstablishmentsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<GetEstablishmentsResponse>> Handle(GetEstablishmentsQuery request, CancellationToken cancellationToken)
{
    var (items, totalCount) = await _repository.GetFilteredAsync(request, cancellationToken);

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
