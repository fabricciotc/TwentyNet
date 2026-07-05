using MediatR;

namespace TwentyNet.Application.Workspaces.ListUserWorkspaces;

public sealed record ListUserWorkspacesQuery : IRequest<IReadOnlyList<WorkspaceDto>>;
