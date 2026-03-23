using Application.CQRS;

namespace Application.UseCases.Court.DeleteCourtGalleryImage;

public record DeleteCourtGalleryImageCommand(
    Guid CourtId,
    string ImageUrl
) : ICommand<bool>;
