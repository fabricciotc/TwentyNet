namespace TwentyNet.BFF.Options;

public sealed class EnrichmentServiceOptions
{
    public const string SectionName = "EnrichmentService";
    public const string ClientName = "EnrichmentClient";

    public string BaseAddress { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;
}
