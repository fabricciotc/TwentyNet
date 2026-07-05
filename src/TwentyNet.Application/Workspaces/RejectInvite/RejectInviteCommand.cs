using MediatR;

namespace TwentyNet.Application.Workspaces.RejectInvite;

public sealed record RejectInviteCommand(Guid Token) : IRequest<Unit>;
