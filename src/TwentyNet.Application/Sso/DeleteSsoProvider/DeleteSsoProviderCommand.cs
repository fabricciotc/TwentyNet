using MediatR;

namespace TwentyNet.Application.Sso.DeleteSsoProvider;

public sealed record DeleteSsoProviderCommand(Guid Id) : IRequest;
