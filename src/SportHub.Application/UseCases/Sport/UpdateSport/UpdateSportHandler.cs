using Application.Common.Interfaces;
using Application.CQRS;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.Sport.UpdateSport;

public class UpdateSportHandler : ICommandHandler<UpdateSportCommand, UpdateSportResponse>
{
    private readonly ISportsRepository _sportsRepository;
    private readonly ILogger<UpdateSportHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateSportHandler(ISportsRepository sportsRepository, ILogger<UpdateSportHandler> logger, IUnitOfWork unitOfWork)
    {
        _sportsRepository = sportsRepository;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UpdateSportResponse>> Handle(UpdateSportCommand request, CancellationToken cancellationToken)
    {
        var sport = await _sportsRepository.GetByIdAsync(request.Id);
        if (sport is null)
            return Result.Fail<UpdateSportResponse>($"Esporte com id '{request.Id}' não encontrado.");

        _logger.LogInformation("Updating sport: {SportId} - {SportName}", request.Id, request.Name);

        sport.Name = request.Name.Trim();
        sport.Description = request.Description?.Trim() ?? sport.Description;
        sport.ImageUrl = request.ImageUrl?.Trim() ?? sport.ImageUrl;

        await _sportsRepository.UpdateAsync(sport);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok(new UpdateSportResponse(sport.Id, sport.Name, sport.Description, sport.ImageUrl));
    }
}
