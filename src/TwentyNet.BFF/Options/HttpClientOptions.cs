namespace TwentyNet.BFF.Options;

public sealed class HttpClientOptions
{
    public const string SectionName = "HttpClient";
    public const string EnrichmentClientName = "EnrichmentClient";

    public string EnrichmentBaseAddress { get; set; } = string.Empty;
    public int EnrichmentTimeoutSeconds { get; set; } = 30;
}
