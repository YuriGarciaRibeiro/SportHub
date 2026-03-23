using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.CQRS;
using MediatR;

namespace Application.UseCases.Location.DeleteLocation;

public class DeleteLocationHandler : ICommandHandler<DeleteLocationCommand, Unit>
{
    private readonly ILocationsRepository _locationsRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteLocationHandler(ILocationsRepository locationsRepository, IUnitOfWork unitOfWork)
    {
        _locationsRepository = locationsRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Unit>> Handle(DeleteLocationCommand request, CancellationToken ct)
    {
        var location = await _locationsRepository.GetByIdAsync(request.Id);
        if (location is null)
            return Result.Fail(new NotFound($"Unidade com ID {request.Id} não encontrada."));

        await _locationsRepository.RemoveAsync(location);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Ok(Unit.Value);
    }
}
