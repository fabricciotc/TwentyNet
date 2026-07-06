namespace TwentyNet.Contracts.Chatbot;

public sealed record ChatSessionResponse(
    Guid Id,
    string Title,
    Guid WorkspaceId,
    Guid UserId,
    DateTime CreatedAt,
    DateTime UpdatedAt);
