using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using TwentyNet.BFF.Options;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.BFF.Services;

public sealed class OpenAiChatbotProvider : IChatbotProvider
{
    private readonly HttpClient _httpClient;
    private readonly AiChatbotOptions _options;
    private readonly ILogger<OpenAiChatbotProvider> _logger;

    public OpenAiChatbotProvider(
        IHttpClientFactory httpClientFactory,
        IOptions<AiChatbotOptions> options,
        ILogger<OpenAiChatbotProvider> logger)
    {
        _options = options.Value;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("AiChatbot");
        _httpClient.BaseAddress = new Uri(_options.BaseAddress);
        _httpClient.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
    }

    public async Task<string> AskAsync(IReadOnlyList<ChatMessageInput> messages, CancellationToken cancellationToken = default)
    {
        var requestBody = new
        {
            model = _options.Model,
            messages = messages.Select(m => new { role = m.Role, content = m.Content }).ToList()
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync("chat/completions", content, cancellationToken);
            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("OpenAI chatbot request failed: {StatusCode} {Response}", (int)response.StatusCode, responseJson);
                return "I'm sorry, I couldn't process your request at this time.";
            }

            using var document = JsonDocument.Parse(responseJson);
            var choice = document.RootElement
                .GetProperty("choices")[0];

            return choice.GetProperty("message").GetProperty("content").GetString()
                ?? "I'm sorry, I didn't get a response.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OpenAI chatbot request threw an exception.");
            return "I'm sorry, I couldn't process your request at this time.";
        }
    }
}
