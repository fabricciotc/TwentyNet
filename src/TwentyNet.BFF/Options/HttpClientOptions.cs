namespace TwentyNet.BFF.Options;

public sealed class HttpClientOptions
{
    public const string SectionName = "HttpClient";
    public const string EnrichmentClientName = "EnrichmentClient";
    public const string WebhookClientName = "WebhookClient";

    public string EnrichmentBaseAddress { get; set; } = string.Empty;
    public int EnrichmentTimeoutSeconds { get; set; } = 30;
    public int WebhookTimeoutSeconds { get; set; } = 30;
    public bool SsrfBlockPrivateNetworks { get; set; } = true;
}
