using MediatR;

namespace TwentyNet.Application.ConnectedAccounts.GetConnectedAccountById;

public sealed record GetConnectedAccountByIdQuery(Guid Id) : IRequest<ConnectedAccountDto?>;
