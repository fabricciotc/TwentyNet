using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Enums;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Workspaces.RemoveMember;

public sealed class RemoveMemberCommandHandler : IRequestHandler<RemoveMemberCommand, Unit>
{
    private readonly IRepository<UserWorkspaceMembership> _membershipRepository;
    private readonly IAuthContext _authContext;

    public RemoveMemberCommandHandler(
        IRepository<UserWorkspaceMembership> membershipRepository,
        IAuthContext authContext)
    {
        _membershipRepository = membershipRepository;
        _authContext = authContext;
    }

    public async Task<Unit> Handle(RemoveMemberCommand request, CancellationToken cancellationToken)
    {
        if (_authContext.Role != WorkspaceRole.Admin)
        {
            throw new UnauthorizedAccessException("Only workspace admins can remove members.");
        }

        if (_authContext.WorkspaceId != request.WorkspaceId)
        {
            throw new UnauthorizedAccessException("Cannot modify members of a different workspace.");
        }

        if (_authContext.UserId == request.UserId)
        {
            throw new InvalidOperationException("Admins cannot remove themselves from the workspace.");
        }

        var memberships = await _membershipRepository.ListAsync(
            m => m.UserId == request.UserId && m.WorkspaceId == request.WorkspaceId,
            cancellationToken);

        var membership = memberships.FirstOrDefault()
            ?? throw new InvalidOperationException("Member not found in workspace.");

        await _membershipRepository.DeleteAsync(membership.Id, cancellationToken);
        await _membershipRepository.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
