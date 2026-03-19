using FluentValidation;

namespace Application.UseCases.Sport.UploadSportImage;

public class UploadSportImageValidator : AbstractValidator<UploadSportImageCommand>
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private static readonly string[] AllowedContentTypes = ["image/jpeg", "image/png", "image/webp"];
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    public UploadSportImageValidator()
    {
        RuleFor(x => x.FileSizeBytes)
            .LessThanOrEqualTo(MaxFileSizeBytes)
            .WithMessage("A imagem não pode ultrapassar 5 MB.");

        RuleFor(x => x.ContentType)
            .Must(ct => AllowedContentTypes.Contains(ct, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Tipo de arquivo inválido. Aceitos: jpg, jpeg, png, webp.");

        RuleFor(x => x.FileName)
            .Must(fn =>
            {
                var ext = Path.GetExtension(fn).ToLowerInvariant();
                return AllowedExtensions.Contains(ext);
            })
            .WithMessage("Extensão de arquivo inválida. Aceitas: .jpg, .jpeg, .png, .webp.");
    }
}
