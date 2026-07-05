using Microsoft.Extensions.Options;
using NSubstitute;
using TwentyNet.BFF.Options;
using TwentyNet.BFF.Services;

namespace TwentyNet.Application.Tests.Webhooks;

public sealed class SecureHttpClientTests
{
    private static SecureHttpClient CreateClient(bool blockPrivateNetworks = true)
    {
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient(Arg.Any<string>()).Returns(new HttpClient());
        var options = Options.Create(new HttpClientOptions
        {
            WebhookTimeoutSeconds = 5,
            SsrfBlockPrivateNetworks = blockPrivateNetworks
        });
        return new SecureHttpClient(factory, options);
    }

    [Theory]
    [InlineData("http://localhost/webhook")]
    [InlineData("http://127.0.0.1/webhook")]
    [InlineData("http://10.0.0.1/webhook")]
    [InlineData("http://172.16.0.1/webhook")]
    [InlineData("http://192.168.1.1/webhook")]
    [InlineData("http://169.254.1.1/webhook")]
    public async Task PostAsync_Should_Block_Private_Addresses(string url)
    {
        // Arrange
        var client = CreateClient();
        using var content = new StringContent("{}");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            client.PostAsync(url, content, CancellationToken.None));

        Assert.Contains("SSRF protection", exception.Message);
    }

    [Fact]
    public async Task PostAsync_Should_Allow_Public_Address_When_Blocking_Enabled()
    {
        // Arrange
        var client = CreateClient();
        using var content = new StringContent("{}");

        // Act & Assert
        // google.com should resolve to public IPs; if DNS resolution fails in the test environment
        // the exception message will indicate resolution failure rather than SSRF.
        var exception = await Record.ExceptionAsync(() =>
            client.PostAsync("https://google.com/webhook", content, CancellationToken.None));

        Assert.True(
            exception is null || exception.Message.Contains("could not resolve host"),
            $"Expected success or DNS resolution failure, but got: {exception?.Message}");
    }

    [Fact]
    public async Task PostAsync_Should_Allow_Private_Address_When_Blocking_Disabled()
    {
        // Arrange
        var client = CreateClient(blockPrivateNetworks: false);
        using var content = new StringContent("{}");

        // Act & Assert
        // localhost still rejected by the URL parser check, but a private IP should be allowed
        // when SSRF blocking is disabled. We just verify no SSRF exception is thrown.
        var exception = await Record.ExceptionAsync(() =>
            client.PostAsync("https://10.0.0.1/webhook", content, CancellationToken.None));

        Assert.False(
            exception?.Message.Contains("SSRF protection") ?? false,
            "SSRF protection should be disabled.");
    }
}
