using MediatR;

namespace TwentyNet.Application.Workspaces.RemoveMember;

public sealed record RemoveMemberCommand(
    Guid WorkspaceId,
    Guid UserId) : IRequest<Unit>;
