namespace TwentyNet.Domain.Interfaces;

public interface ISecureHttpClient
{
    Task<HttpResponseMessage> PostAsync(string url, HttpContent content, CancellationToken cancellationToken = default);
}
