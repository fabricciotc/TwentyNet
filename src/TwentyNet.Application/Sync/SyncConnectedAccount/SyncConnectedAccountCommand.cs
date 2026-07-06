using MediatR;

namespace TwentyNet.Application.Sync.SyncConnectedAccount;

public sealed record SyncConnectedAccountCommand(Guid ConnectedAccountId) : IRequest<SyncResult>;

public sealed record SyncResult(int EmailsSynced, int EventsSynced);
