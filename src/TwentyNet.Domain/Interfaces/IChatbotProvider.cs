namespace TwentyNet.Domain.Interfaces;

public interface IChatbotProvider
{
    Task<string> AskAsync(IReadOnlyList<ChatMessageInput> messages, CancellationToken cancellationToken = default);
}

public sealed record ChatMessageInput(string Role, string Content);
