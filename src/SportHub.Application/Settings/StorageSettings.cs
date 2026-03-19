namespace Application.Settings;

public class StorageSettings
{
    public string BucketName { get; set; } = default!;
    public string Region { get; set; } = "us-east-1";

    /// <summary>Override de endpoint para MinIO em dev. Vazio = endpoint padrão AWS S3.</summary>
    public string? ServiceUrl { get; set; }

    public string AccessKey { get; set; } = default!;
    public string SecretKey { get; set; } = default!;

    /// <summary>
    /// Base URL para construir URLs públicas de imagens.
    /// Dev: http://localhost:9000/sporthub-courts
    /// Prod: https://sporthub-courts.s3.amazonaws.com (ou CloudFront)
    /// </summary>
    public string PublicBaseUrl { get; set; } = default!;
}
