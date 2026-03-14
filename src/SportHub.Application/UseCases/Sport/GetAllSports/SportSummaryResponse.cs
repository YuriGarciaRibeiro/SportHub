namespace Application.UseCases.Sport.GetAllSports;

public record SportSummaryResponse(Guid Id, string Name, string Description, string ImageUrl);
