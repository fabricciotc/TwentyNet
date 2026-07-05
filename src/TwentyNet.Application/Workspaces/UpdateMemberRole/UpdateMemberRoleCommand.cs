using MediatR;
using TwentyNet.Domain.Enums;

namespace TwentyNet.Application.Workspaces.UpdateMemberRole;

public sealed record UpdateMemberRoleCommand(
    Guid WorkspaceId,
    Guid UserId,
    WorkspaceRole Role) : IRequest<Unit>;
