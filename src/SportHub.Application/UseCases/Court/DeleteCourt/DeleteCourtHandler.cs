using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.CQRS;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.Court.DeleteCourt;

public class DeleteCourtHandler : ICommandHandler<DeleteCourtCommand>
{
    private readonly ICourtsRepository _courtsRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteCourtHandler> _logger;

    public DeleteCourtHandler(
        ICourtsRepository courtsRepository,
        IUnitOfWork unitOfWork,
        ILogger<DeleteCourtHandler> logger)
    {
        _courtsRepository = courtsRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteCourtCommand request, CancellationToken cancellationToken)
    {
        var court = await _courtsRepository.GetByIdAsync(request.Id);

        if (court is null)
            return Result.Fail(new NotFound($"Quadra com ID {request.Id} não encontrada."));

        _logger.LogInformation("Removendo quadra: {CourtId} - {CourtName}", request.Id, court.Name);

        await _courtsRepository.RemoveAsync(court);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
