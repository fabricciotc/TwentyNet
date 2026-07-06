using TwentyNet.Domain.Common;
using TwentyNet.Domain.Enums;

namespace TwentyNet.Domain.Entities;

public sealed class SsoProvider : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public Guid WorkspaceId { get; set; }
    public Workspace Workspace { get; set; } = null!;
    public SsoProviderType Type { get; set; }
    public bool IsActive { get; set; } = true;

    // Common OAuth
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string? AuthorizationEndpoint { get; set; }
    public string? TokenEndpoint { get; set; }
    public string? UserInfoEndpoint { get; set; }

    // SAML
    public string? EntityId { get; set; }
    public string? SingleSignOnUrl { get; set; }
    public string? Certificate { get; set; }
    public string? MetadataUrl { get; set; }
}
