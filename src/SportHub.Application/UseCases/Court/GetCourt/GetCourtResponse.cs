namespace Application.UseCases.Court.GetCourt;

public class GetCourtResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public TimeOnly OpeningTime { get; set; }
    public TimeOnly ClosingTime { get; set; }
    public string TimeZone { get; set; } = "America/Maceio";
    public IEnumerable<SportDto> Sports { get; set; } = Enumerable.Empty<SportDto>();
    public EstablishmentDto Establishment { get; set; } = new EstablishmentDto();

}

public class SportDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class EstablishmentDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
}