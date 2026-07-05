using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Workspaces.GetWorkspaceMembers;

public sealed class GetWorkspaceMembersQueryHandler : IRequestHandler<GetWorkspaceMembersQuery, IReadOnlyList<WorkspaceMemberDto>>
{
    private readonly IRepository<UserWorkspaceMembership> _membershipRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IAuthContext _authContext;

    public GetWorkspaceMembersQueryHandler(
        IRepository<UserWorkspaceMembership> membershipRepository,
        IRepository<User> userRepository,
        IAuthContext authContext)
    {
        _membershipRepository = membershipRepository;
        _userRepository = userRepository;
        _authContext = authContext;
    }

    public async Task<IReadOnlyList<WorkspaceMemberDto>> Handle(GetWorkspaceMembersQuery request, CancellationToken cancellationToken)
    {
        if (!_authContext.UserId.HasValue || !_authContext.WorkspaceId.HasValue)
        {
            throw new UnauthorizedAccessException("User and workspace are required.");
        }

        if (request.WorkspaceId != _authContext.WorkspaceId.Value)
        {
            throw new UnauthorizedAccessException("Cannot access members of a different workspace.");
        }

        var memberships = await _membershipRepository.ListAsync(
            m => m.WorkspaceId == request.WorkspaceId,
            cancellationToken);

        var userIds = memberships.Select(m => m.UserId).ToList();
        var users = await _userRepository.ListAsync(
            u => userIds.Contains(u.Id),
            cancellationToken);

        var userById = users.ToDictionary(u => u.Id);

        return memberships
            .Where(m => userById.ContainsKey(m.UserId))
            .Select(m => new WorkspaceMemberDto(
                m.UserId,
                userById[m.UserId].Email.Value,
                userById[m.UserId].FirstName,
                userById[m.UserId].LastName,
                m.Role))
            .ToList();
    }
}
