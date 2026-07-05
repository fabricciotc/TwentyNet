using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Workspaces.ListUserWorkspaces;

public sealed class ListUserWorkspacesQueryHandler : IRequestHandler<ListUserWorkspacesQuery, IReadOnlyList<WorkspaceDto>>
{
    private readonly IRepository<UserWorkspaceMembership> _membershipRepository;
    private readonly IRepository<Workspace> _workspaceRepository;
    private readonly IAuthContext _authContext;

    public ListUserWorkspacesQueryHandler(
        IRepository<UserWorkspaceMembership> membershipRepository,
        IRepository<Workspace> workspaceRepository,
        IAuthContext authContext)
    {
        _membershipRepository = membershipRepository;
        _workspaceRepository = workspaceRepository;
        _authContext = authContext;
    }

    public async Task<IReadOnlyList<WorkspaceDto>> Handle(ListUserWorkspacesQuery request, CancellationToken cancellationToken)
    {
        if (!_authContext.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User is required.");
        }

        var userId = _authContext.UserId.Value;

        var memberships = await _membershipRepository.ListAsync(
            m => m.UserId == userId,
            cancellationToken);

        var workspaceIds = memberships.Select(m => m.WorkspaceId).ToList();
        var workspaces = await _workspaceRepository.ListAsync(
            w => workspaceIds.Contains(w.Id),
            cancellationToken);

        var workspaceById = workspaces.ToDictionary(w => w.Id);

        return memberships
            .Where(m => workspaceById.ContainsKey(m.WorkspaceId))
            .Select(m => new WorkspaceDto(
                m.WorkspaceId,
                workspaceById[m.WorkspaceId].Name,
                m.Role,
                workspaceById[m.WorkspaceId].CreatedAt))
            .ToList();
    }
}
