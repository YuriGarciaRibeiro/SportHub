using Application.Common.Interfaces;
using Application.CQRS;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.Sport.DeleteSport;

public class DeleteSportHandler : ICommandHandler<DeleteSportCommand>
{
    private readonly ISportsRepository _sportsRepository;
    private readonly ILogger<DeleteSportHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteSportHandler(ISportsRepository sportsRepository, ILogger<DeleteSportHandler> logger, IUnitOfWork unitOfWork)
    {
        _sportsRepository = sportsRepository;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteSportCommand request, CancellationToken cancellationToken)
    {
        var sport = await _sportsRepository.GetByIdAsync(request.Id);
        if (sport is null)
            return Result.Fail($"Esporte com id '{request.Id}' não encontrado.");

        _logger.LogInformation("Deleting sport: {SportId}", request.Id);

        await _sportsRepository.RemoveAsync(sport);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
