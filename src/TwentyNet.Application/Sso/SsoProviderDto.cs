using TwentyNet.Domain.Enums;

namespace TwentyNet.Application.Sso;

public sealed record SsoProviderDto(
    Guid Id,
    string Name,
    Guid WorkspaceId,
    SsoProviderType Type,
    bool IsActive,
    string? ClientId,
    string? AuthorizationEndpoint,
    string? TokenEndpoint,
    string? UserInfoEndpoint,
    string? EntityId,
    string? SingleSignOnUrl,
    string? MetadataUrl,
    DateTime CreatedAt,
    DateTime UpdatedAt);
