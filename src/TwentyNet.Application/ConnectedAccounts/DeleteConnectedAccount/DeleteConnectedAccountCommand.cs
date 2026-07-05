using MediatR;

namespace TwentyNet.Application.ConnectedAccounts.DeleteConnectedAccount;

public sealed record DeleteConnectedAccountCommand(Guid Id) : IRequest<Unit>;
