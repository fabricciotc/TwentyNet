using TwentyNet.Domain.Enums;

namespace TwentyNet.Application.ConnectedAccounts;

public sealed record ConnectedAccountDto(
    Guid Id,
    Guid UserId,
    Guid WorkspaceId,
    ConnectorProvider Provider,
    string Email,
    DateTime? ExpiresAt,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);
