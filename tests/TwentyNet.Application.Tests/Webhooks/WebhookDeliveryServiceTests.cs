using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using TwentyNet.Application.Webhooks;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;
using TwentyNet.Domain.Webhooks;

namespace TwentyNet.Application.Tests.Webhooks;

public sealed class WebhookDeliveryServiceTests
{
    [Fact]
    public void ComputeSignature_Should_Return_Prefixed_Hex_Hash()
    {
        // Arrange
        const string secret = "super-secret";
        const string payload = "{\"event\":\"company.created\"}";

        // Act
        var signature = WebhookDeliveryService.ComputeSignature(secret, payload);

        // Assert
        Assert.StartsWith("sha256=", signature);
        var hex = signature["sha256=".Length..];
        Assert.Equal(64, hex.Length);
    }

    [Fact]
    public async Task DeliverAsync_Should_Post_Json_With_Signature_And_Event_Headers()
    {
        // Arrange
        var secureHttpClient = Substitute.For<ISecureHttpClient>();
        secureHttpClient.PostAsync(Arg.Any<string>(), Arg.Any<HttpContent>(), Arg.Any<CancellationToken>())
            .Returns(new HttpResponseMessage(System.Net.HttpStatusCode.OK));

        var service = new WebhookDeliveryService(secureHttpClient, NullLogger<WebhookDeliveryService>.Instance);

        var webhook = new Webhook
        {
            Id = Guid.NewGuid(),
            TargetUrl = "https://example.com/webhook",
            Secret = "secret",
            Events = new List<string> { "company.created" }
        };

        var payload = new WebhookPayload(
            "company.created",
            Guid.NewGuid(),
            "Company",
            Guid.NewGuid(),
            DateTime.UtcNow,
            new { recordId = Guid.NewGuid() });

        // Act
        var result = await service.DeliverAsync(webhook, payload, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);

        var call = Assert.Single(secureHttpClient.ReceivedCalls()
            .Where(c => c.GetMethodInfo().Name == nameof(ISecureHttpClient.PostAsync)));

        var args = call.GetArguments();
        Assert.Equal(webhook.TargetUrl, args[0]);
        var content = Assert.IsType<StringContent>(args[1]);
        content.Headers.TryGetValues("X-Twenty-Webhook-Signature", out var signatureValues);
        content.Headers.TryGetValues("X-Twenty-Webhook-Event", out var eventValues);

        Assert.NotNull(signatureValues);
        Assert.Single(signatureValues);
        Assert.NotNull(eventValues);
        Assert.Single(eventValues);
        Assert.Equal("company.created", eventValues!.First());

        var json = await content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        Assert.Equal("company.created", doc.RootElement.GetProperty("event").GetString());
    }

    [Fact]
    public async Task DeliverAsync_Should_Return_Failure_When_Post_Returns_Error()
    {
        // Arrange
        var secureHttpClient = Substitute.For<ISecureHttpClient>();
        secureHttpClient.PostAsync(Arg.Any<string>(), Arg.Any<HttpContent>(), Arg.Any<CancellationToken>())
            .Returns(new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest));

        var service = new WebhookDeliveryService(secureHttpClient, NullLogger<WebhookDeliveryService>.Instance);

        var webhook = new Webhook
        {
            Id = Guid.NewGuid(),
            TargetUrl = "https://example.com/webhook",
            Secret = "secret",
            Events = new List<string> { "company.created" }
        };

        var payload = new WebhookPayload(
            "company.created",
            Guid.NewGuid(),
            "Company",
            Guid.NewGuid(),
            DateTime.UtcNow,
            new { recordId = Guid.NewGuid() });

        // Act
        var result = await service.DeliverAsync(webhook, payload, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
    }
}
