using MediatR;

namespace TwentyNet.Application.Workspaces.GetWorkspaceMembers;

public sealed record GetWorkspaceMembersQuery(Guid WorkspaceId) : IRequest<IReadOnlyList<WorkspaceMemberDto>>;
