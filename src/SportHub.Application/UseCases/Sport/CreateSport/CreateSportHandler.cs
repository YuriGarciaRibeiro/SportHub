using Application.Common.Interfaces;
using Application.CQRS;
using Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.Sport.CreateSport;

public class CreateSportHandler : ICommandHandler<CreateSportCommand, CreateSportResponse>
{
    private readonly ISportsRepository _sportsRepository;
    private readonly ILogger<CreateSportHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSportHandler(ISportsRepository sportsRepository, ILogger<CreateSportHandler> logger, IUnitOfWork unitOfWork)
    {
        _sportsRepository = sportsRepository;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CreateSportResponse>> Handle(CreateSportCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating sport: {SportName}", request.Name);

        if (await _sportsRepository.ExistsByNameAsync(request.Name))
            return Result.Fail<CreateSportResponse>($"Já existe um esporte com o nome '{request.Name}'.");

        var sport = new Domain.Entities.Sport
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim() ?? "",
            ImageUrl = request.ImageUrl?.Trim() ?? "",
        };

        await _sportsRepository.AddAsync(sport);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok(new CreateSportResponse(sport.Id, sport.Name, sport.Description, sport.ImageUrl));
    }
}
