using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.CQRS;
using Domain.Enums;

namespace Application.UseCases.CourtMaintenance.CreateCourtMaintenance;

public class CreateCourtMaintenanceHandler : ICommandHandler<CreateCourtMaintenanceCommand, Guid>
{
    private readonly ICourtsRepository _courtsRepository;
    private readonly ICourtMaintenanceRepository _maintenanceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCourtMaintenanceHandler(
        ICourtsRepository courtsRepository,
        ICourtMaintenanceRepository maintenanceRepository,
        IUnitOfWork unitOfWork)
    {
        _courtsRepository = courtsRepository;
        _maintenanceRepository = maintenanceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateCourtMaintenanceCommand request, CancellationToken cancellationToken)
    {
        var courtExists = await _courtsRepository.ExistsAsync(request.CourtId);
        if (!courtExists)
            return Result.Fail(new NotFound($"Quadra com ID {request.CourtId} não encontrada."));

        var maintenance = new Domain.Entities.CourtMaintenance
        {
            CourtId = request.CourtId,
            Type = request.Type,
            Description = request.Description,
            DayOfWeek = request.DayOfWeek,
            Date = request.Date,
            StartTime = request.StartTime,
            EndTime = request.EndTime
        };

        await _maintenanceRepository.AddAsync(maintenance);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok(maintenance.Id);
    }
}
