namespace TwentyNet.Contracts.Sso;

public sealed record SsoProviderResponse(
    Guid Id,
    string Name,
    Guid WorkspaceId,
    string Type,
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
