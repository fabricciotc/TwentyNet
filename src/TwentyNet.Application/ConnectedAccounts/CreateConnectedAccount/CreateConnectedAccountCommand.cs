using MediatR;
using TwentyNet.Domain.Enums;

namespace TwentyNet.Application.ConnectedAccounts.CreateConnectedAccount;

public sealed record CreateConnectedAccountCommand(
    ConnectorProvider Provider,
    string Email,
    string AccessToken,
    string RefreshToken,
    DateTime? ExpiresAt) : IRequest<ConnectedAccountDto>;
