using MediatR;

namespace TwentyNet.Application.ConnectedAccounts.ListConnectedAccounts;

public sealed record ListConnectedAccountsQuery : IRequest<IReadOnlyList<ConnectedAccountDto>>;
