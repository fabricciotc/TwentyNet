namespace TwentyNet.BFF.Options;

public sealed class WebhookServiceOptions
{
    public const string SectionName = "WebhookService";
    public const string ClientName = "WebhookClient";

    public int TimeoutSeconds { get; set; } = 30;
    public bool SsrfBlockPrivateNetworks { get; set; } = true;
}
