using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Enums;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Workspaces.UpdateMemberRole;

public sealed class UpdateMemberRoleCommandHandler : IRequestHandler<UpdateMemberRoleCommand, Unit>
{
    private readonly IRepository<UserWorkspaceMembership> _membershipRepository;
    private readonly IAuthContext _authContext;

    public UpdateMemberRoleCommandHandler(
        IRepository<UserWorkspaceMembership> membershipRepository,
        IAuthContext authContext)
    {
        _membershipRepository = membershipRepository;
        _authContext = authContext;
    }

    public async Task<Unit> Handle(UpdateMemberRoleCommand request, CancellationToken cancellationToken)
    {
        if (_authContext.Role != WorkspaceRole.Admin)
        {
            throw new UnauthorizedAccessException("Only workspace admins can update member roles.");
        }

        if (_authContext.WorkspaceId != request.WorkspaceId)
        {
            throw new UnauthorizedAccessException("Cannot modify members of a different workspace.");
        }

        var memberships = await _membershipRepository.ListAsync(
            m => m.UserId == request.UserId && m.WorkspaceId == request.WorkspaceId,
            cancellationToken);

        var membership = memberships.FirstOrDefault()
            ?? throw new InvalidOperationException("Member not found in workspace.");

        membership.Role = request.Role;
        _membershipRepository.Update(membership);
        await _membershipRepository.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
