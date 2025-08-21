namespace Application.UseCases.Sports.GetAllSports;

public class GetAllSportsResponse
{
    public IEnumerable<SportDto> Sports { get; set; } = null!;
}

public class SportDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}