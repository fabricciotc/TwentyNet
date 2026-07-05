namespace TwentyNet.Domain.Interfaces;

public interface IStorageDriver
{
    Task<Stream> ReadAsync(string key, CancellationToken cancellationToken = default);
    Task WriteAsync(string key, Stream content, string contentType, CancellationToken cancellationToken = default);
    Task DeleteAsync(string key, CancellationToken cancellationToken = default);
    Task<string?> GetPresignedUploadUrlAsync(string key, TimeSpan expiry, CancellationToken cancellationToken = default);
    Task<string?> GetPresignedDownloadUrlAsync(string key, TimeSpan expiry, CancellationToken cancellationToken = default);
}
