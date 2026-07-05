using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using TwentyNet.BFF.Options;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.BFF.Services;

public sealed class S3StorageDriver : IStorageDriver
{
    private readonly IAmazonS3 _client;
    private readonly StorageOptions _options;

    public S3StorageDriver(IAmazonS3 client, IOptions<StorageOptions> options)
    {
        _client = client;
        _options = options.Value;
    }

    public async Task<Stream> ReadAsync(string key, CancellationToken cancellationToken = default)
    {
        var response = await _client.GetObjectAsync(_options.S3Bucket, key, cancellationToken);
        return response.ResponseStream;
    }

    public async Task WriteAsync(string key, Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        var request = new PutObjectRequest
        {
            BucketName = _options.S3Bucket,
            Key = key,
            InputStream = content,
            ContentType = contentType,
            AutoCloseStream = false
        };

        await _client.PutObjectAsync(request, cancellationToken);
    }

    public async Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        await _client.DeleteObjectAsync(_options.S3Bucket, key, cancellationToken);
    }

    public Task<string?> GetPresignedUploadUrlAsync(string key, TimeSpan expiry, CancellationToken cancellationToken = default)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _options.S3Bucket,
            Key = key,
            Verb = HttpVerb.PUT,
            Expires = DateTime.UtcNow.Add(expiry)
        };

        var url = _client.GetPreSignedURL(request);
        return Task.FromResult<string?>(url);
    }

    public Task<string?> GetPresignedDownloadUrlAsync(string key, TimeSpan expiry, CancellationToken cancellationToken = default)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _options.S3Bucket,
            Key = key,
            Verb = HttpVerb.GET,
            Expires = DateTime.UtcNow.Add(expiry)
        };

        var url = _client.GetPreSignedURL(request);
        return Task.FromResult<string?>(url);
    }
}
