namespace TwentyNet.BFF.Options;

public sealed class AiChatbotOptions
{
    public const string SectionName = "AiChatbot";

    public string Provider { get; set; } = "Stub";
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gpt-4o-mini";
    public string BaseAddress { get; set; } = "https://api.openai.com/v1/";
    public int TimeoutSeconds { get; set; } = 60;
}
