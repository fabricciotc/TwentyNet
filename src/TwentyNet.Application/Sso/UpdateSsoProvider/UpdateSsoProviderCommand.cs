using MediatR;
using TwentyNet.Domain.Enums;

namespace TwentyNet.Application.Sso.UpdateSsoProvider;

public sealed record UpdateSsoProviderCommand(
    Guid Id,
    string Name,
    bool IsActive,
    SsoProviderType Type,
    string? ClientId,
    string? ClientSecret,
    string? AuthorizationEndpoint,
    string? TokenEndpoint,
    string? UserInfoEndpoint,
    string? EntityId,
    string? SingleSignOnUrl,
    string? Certificate,
    string? MetadataUrl) : IRequest<SsoProviderDto>;
