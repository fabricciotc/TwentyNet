namespace TwentyNet.Contracts.ApiKeys;

public sealed record ApiKeyResponse(
    Guid Id,
    string Name,
    string KeyPrefix,
    Guid WorkspaceId,
    string Role,
    IReadOnlyList<string> Scopes,
    DateTime? ExpiresAt,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record ApiKeyCreatedResponse(
    Guid Id,
    string Name,
    string PlainKey,
    Guid WorkspaceId,
    DateTime ExpiresAt);
