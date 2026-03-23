using Application.CQRS;
using Application.UseCases.Location.CreateLocation;

namespace Application.UseCases.Location.UpdateLocation;

public class UpdateLocationCommand : ICommand<Unit>
{
    public Guid Id { get; set; }
    public LocationRequest Location { get; set; } = new LocationRequest();
}
