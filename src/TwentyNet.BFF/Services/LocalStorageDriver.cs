using Microsoft.Extensions.Options;
using TwentyNet.BFF.Options;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.BFF.Services;

public sealed class LocalStorageDriver : IStorageDriver
{
    private readonly StorageOptions _options;

    public LocalStorageDriver(IOptions<StorageOptions> options)
    {
        _options = options.Value;
    }

    public Task<Stream> ReadAsync(string key, CancellationToken cancellationToken = default)
    {
        var path = GetFullPath(key);
        if (!System.IO.File.Exists(path))
        {
            throw new FileNotFoundException("File not found.", path);
        }

        return Task.FromResult<Stream>(System.IO.File.OpenRead(path));
    }

    public async Task WriteAsync(string key, Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        var path = GetFullPath(key);
        var directory = System.IO.Path.GetDirectoryName(path);

        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var fileStream = System.IO.File.Create(path);
        await content.CopyToAsync(fileStream, cancellationToken);
    }

    public Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        var path = GetFullPath(key);
        if (System.IO.File.Exists(path))
        {
            System.IO.File.Delete(path);
        }

        return Task.CompletedTask;
    }

    public Task<string?> GetPresignedUploadUrlAsync(string key, TimeSpan expiry, CancellationToken cancellationToken = default)
        => Task.FromResult<string?>(null);

    public Task<string?> GetPresignedDownloadUrlAsync(string key, TimeSpan expiry, CancellationToken cancellationToken = default)
        => Task.FromResult<string?>(null);

    private string GetFullPath(string key)
    {
        // Prevent directory traversal
        var safeKey = key.Replace("..", string.Empty).TrimStart('/', '\\');
        return System.IO.Path.Combine(_options.LocalPath, safeKey);
    }
}
