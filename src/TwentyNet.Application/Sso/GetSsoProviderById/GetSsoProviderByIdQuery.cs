using MediatR;

namespace TwentyNet.Application.Sso.GetSsoProviderById;

public sealed record GetSsoProviderByIdQuery(Guid Id) : IRequest<SsoProviderDto?>;
