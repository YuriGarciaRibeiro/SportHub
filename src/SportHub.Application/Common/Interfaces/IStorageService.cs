namespace Application.Common.Interfaces;

public interface IStorageService
{
    /// <summary>
    /// Faz upload de um stream para o object storage e retorna a URL pública.
    /// </summary>
    Task<string> UploadAsync(string key, Stream stream, string contentType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove um objeto pelo key. No-op se não existir.
    /// </summary>
    Task DeleteAsync(string key, CancellationToken cancellationToken = default);
}
