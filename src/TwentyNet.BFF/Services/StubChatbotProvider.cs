using TwentyNet.Domain.Interfaces;

namespace TwentyNet.BFF.Services;

public sealed class StubChatbotProvider : IChatbotProvider
{
    public Task<string> AskAsync(IReadOnlyList<ChatMessageInput> messages, CancellationToken cancellationToken = default)
    {
        var lastUserMessage = messages.LastOrDefault(m => m.Role == "user")?.Content ?? string.Empty;
        return Task.FromResult($"This is a stub response to: '{lastUserMessage}'");
    }
}
