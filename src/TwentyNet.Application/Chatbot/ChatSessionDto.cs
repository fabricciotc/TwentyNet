namespace TwentyNet.Application.Chatbot;

public sealed record ChatSessionDto(
    Guid Id,
    string Title,
    Guid WorkspaceId,
    Guid UserId,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record ChatMessageDto(
    Guid Id,
    Guid SessionId,
    string Role,
    string Content,
    DateTime CreatedAt,
    DateTime UpdatedAt);
