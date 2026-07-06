using TwentyNet.Domain.Enums;

namespace TwentyNet.Application.ApiKeys;

public sealed record ApiKeyDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string KeyPrefix { get; init; } = string.Empty;
    public Guid WorkspaceId { get; init; }
    public WorkspaceRole Role { get; init; }
    public IReadOnlyList<string> Scopes { get; init; } = Array.Empty<string>();
    public DateTime? ExpiresAt { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public sealed record ApiKeyCreatedDto(
    Guid Id,
    string Name,
    string PlainKey,
    Guid WorkspaceId,
    DateTime ExpiresAt);
