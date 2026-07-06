namespace TwentyNet.Contracts.Chatbot;

public sealed record ChatMessageResponse(
    Guid Id,
    Guid SessionId,
    string Role,
    string Content,
    DateTime CreatedAt,
    DateTime UpdatedAt);
