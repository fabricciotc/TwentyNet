using MediatR;
using TwentyNet.Domain.Enums;

namespace TwentyNet.Application.Sso.CreateSsoProvider;

public sealed record CreateSsoProviderCommand(
    string Name,
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
