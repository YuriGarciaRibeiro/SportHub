namespace Application.UseCases.Establishments.GetEstablishmentSports;

public class GetEstablishmentSportsResponse
{
    public IEnumerable<SportDto> Sports { get; set; } = null!;
}

public class SportDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}