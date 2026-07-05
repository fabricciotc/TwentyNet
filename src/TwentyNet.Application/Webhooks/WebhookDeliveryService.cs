using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;
using TwentyNet.Domain.Webhooks;

namespace TwentyNet.Application.Webhooks;

public sealed class WebhookDeliveryService : IWebhookDeliveryService
{
    private readonly ISecureHttpClient _secureHttpClient;
    private readonly ILogger<WebhookDeliveryService> _logger;

    public WebhookDeliveryService(ISecureHttpClient secureHttpClient, ILogger<WebhookDeliveryService> logger)
    {
        _secureHttpClient = secureHttpClient;
        _logger = logger;
    }

    public async Task<WebhookDeliveryResult> DeliverAsync(Webhook webhook, WebhookPayload payload, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
        });

        var signature = ComputeSignature(webhook.Secret, json);

        var content = new StringContent(json, Encoding.UTF8, "application/json");
        content.Headers.TryAddWithoutValidation("X-Twenty-Webhook-Signature", signature);
        content.Headers.TryAddWithoutValidation("X-Twenty-Webhook-Event", payload.Event);

        try
        {
            var response = await _secureHttpClient.PostAsync(webhook.TargetUrl, content, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return new WebhookDeliveryResult(true, (int)response.StatusCode);
            }

            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning(
                "Webhook {WebhookId} delivery failed with status {StatusCode}. Response: {Response}",
                webhook.Id,
                (int)response.StatusCode,
                error);

            return new WebhookDeliveryResult(false, (int)response.StatusCode, error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Webhook {WebhookId} delivery threw an exception.", webhook.Id);
            return new WebhookDeliveryResult(false, ErrorMessage: ex.Message);
        }
    }

    public static string ComputeSignature(string secret, string payload)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return $"sha256={Convert.ToHexString(hash).ToLowerInvariant()}";
    }
}
