using MediatR;

namespace TwentyNet.Application.Workspaces.AcceptInvite;

public sealed record AcceptInviteCommand(Guid Token) : IRequest<Unit>;
