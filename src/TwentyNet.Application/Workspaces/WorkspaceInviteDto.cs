using TwentyNet.Domain.Enums;

namespace TwentyNet.Application.Workspaces;

public sealed record WorkspaceInviteDto(
    Guid Id,
    Guid WorkspaceId,
    string Email,
    WorkspaceRole Role,
    string Token,
    DateTime ExpiresAt,
    DateTime? AcceptedAt,
    DateTime? RejectedAt);
