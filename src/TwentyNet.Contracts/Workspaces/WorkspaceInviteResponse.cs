namespace TwentyNet.Contracts.Workspaces;

public sealed record WorkspaceInviteResponse(
    Guid Id,
    Guid WorkspaceId,
    string Email,
    string Role,
    string Token,
    DateTime ExpiresAt,
    DateTime? AcceptedAt,
    DateTime? RejectedAt);
