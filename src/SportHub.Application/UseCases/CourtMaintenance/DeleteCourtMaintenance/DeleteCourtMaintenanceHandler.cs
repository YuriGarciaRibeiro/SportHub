using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.CQRS;

namespace Application.UseCases.CourtMaintenance.DeleteCourtMaintenance;

public class DeleteCourtMaintenanceHandler : ICommandHandler<DeleteCourtMaintenanceCommand>
{
    private readonly ICourtMaintenanceRepository _maintenanceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCourtMaintenanceHandler(
        ICourtMaintenanceRepository maintenanceRepository,
        IUnitOfWork unitOfWork)
    {
        _maintenanceRepository = maintenanceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteCourtMaintenanceCommand request, CancellationToken cancellationToken)
    {
        var maintenance = await _maintenanceRepository.GetByIdAsync(request.MaintenanceId);

        if (maintenance is null || maintenance.CourtId != request.CourtId)
            return Result.Fail(new NotFound($"Manutenção com ID {request.MaintenanceId} não encontrada."));

        await _maintenanceRepository.RemoveAsync(maintenance);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
