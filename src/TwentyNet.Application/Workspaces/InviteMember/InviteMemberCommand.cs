using MediatR;
using TwentyNet.Domain.Enums;

namespace TwentyNet.Application.Workspaces.InviteMember;

public sealed record InviteMemberCommand(
    Guid WorkspaceId,
    string Email,
    WorkspaceRole Role) : IRequest<WorkspaceInviteDto>;
