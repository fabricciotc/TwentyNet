using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Options;
using TwentyNet.BFF.Options;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.BFF.Services;

public sealed class SecureHttpClient : ISecureHttpClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly WebhookServiceOptions _options;

    public SecureHttpClient(IHttpClientFactory httpClientFactory, IOptions<WebhookServiceOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
    }

    public async Task<HttpResponseMessage> PostAsync(string url, HttpContent content, CancellationToken cancellationToken = default)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            throw new ArgumentException("Invalid URL.", nameof(url));
        }

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        {
            throw new ArgumentException("Only HTTP and HTTPS URLs are allowed.", nameof(url));
        }

        if (_options.SsrfBlockPrivateNetworks)
        {
            await BlockPrivateNetworksAsync(uri, cancellationToken);
        }

        var client = _httpClientFactory.CreateClient(WebhookServiceOptions.ClientName);
        client.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);

        return await client.PostAsync(uri, content, cancellationToken);
    }

    private static async Task BlockPrivateNetworksAsync(Uri uri, CancellationToken cancellationToken)
    {
        var host = uri.Host;

        if (string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("SSRF protection: localhost is not allowed.");
        }

        var addresses = await Dns.GetHostAddressesAsync(host, cancellationToken);

        if (addresses.Length == 0)
        {
            throw new InvalidOperationException($"SSRF protection: could not resolve host '{host}'.");
        }

        foreach (var address in addresses)
        {
            if (IsPrivateAddress(address))
            {
                throw new InvalidOperationException($"SSRF protection: address '{address}' for host '{host}' is in a private network range.");
            }
        }
    }

    private static bool IsPrivateAddress(IPAddress address)
    {
        if (address.AddressFamily == AddressFamily.InterNetworkV6)
        {
            if (IPAddress.IsLoopback(address))
            {
                return true;
            }

            var bytes = address.GetAddressBytes();

            // fc00::/7
            if ((bytes[0] & 0xFE) == 0xFC)
            {
                return true;
            }

            return false;
        }

        if (address.AddressFamily == AddressFamily.InterNetwork)
        {
            var bytes = address.GetAddressBytes();

            // 127.0.0.0/8
            if (bytes[0] == 127)
            {
                return true;
            }

            // 10.0.0.0/8
            if (bytes[0] == 10)
            {
                return true;
            }

            // 172.16.0.0/12
            if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
            {
                return true;
            }

            // 192.168.0.0/16
            if (bytes[0] == 192 && bytes[1] == 168)
            {
                return true;
            }

            // 169.254.0.0/16
            if (bytes[0] == 169 && bytes[1] == 254)
            {
                return true;
            }

            return false;
        }

        return false;
    }
}
