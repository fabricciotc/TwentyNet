namespace TwentyNet.Contracts.Sso;

public sealed record CreateSsoProviderRequest(
    string Name,
    string Type,
    string? ClientId = null,
    string? ClientSecret = null,
    string? AuthorizationEndpoint = null,
    string? TokenEndpoint = null,
    string? UserInfoEndpoint = null,
    string? EntityId = null,
    string? SingleSignOnUrl = null,
    string? Certificate = null,
    string? MetadataUrl = null);
