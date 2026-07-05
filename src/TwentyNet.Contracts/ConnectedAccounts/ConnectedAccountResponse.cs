namespace TwentyNet.Contracts.ConnectedAccounts;

public sealed record ConnectedAccountResponse(
    Guid Id,
    Guid UserId,
    Guid WorkspaceId,
    string Provider,
    string Email,
    DateTime? ExpiresAt,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);
