using MediatR;

namespace TwentyNet.Application.Sso.ListSsoProviders;

public sealed record ListSsoProvidersQuery : IRequest<IReadOnlyList<SsoProviderDto>>;
