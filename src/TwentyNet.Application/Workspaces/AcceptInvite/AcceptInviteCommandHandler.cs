using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Workspaces.AcceptInvite;

public sealed class AcceptInviteCommandHandler : IRequestHandler<AcceptInviteCommand, Unit>
{
    private readonly IRepository<WorkspaceInvite> _inviteRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<UserWorkspaceMembership> _membershipRepository;
    private readonly IAuthContext _authContext;

    public AcceptInviteCommandHandler(
        IRepository<WorkspaceInvite> inviteRepository,
        IRepository<User> userRepository,
        IRepository<UserWorkspaceMembership> membershipRepository,
        IAuthContext authContext)
    {
        _inviteRepository = inviteRepository;
        _userRepository = userRepository;
        _membershipRepository = membershipRepository;
        _authContext = authContext;
    }

    public async Task<Unit> Handle(AcceptInviteCommand request, CancellationToken cancellationToken)
    {
        if (!_authContext.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User must be authenticated to accept an invite.");
        }

        var invites = await _inviteRepository.ListAsync(
            i => i.Token == request.Token.ToString("N"),
            cancellationToken);

        var invite = invites.FirstOrDefault()
            ?? throw new InvalidOperationException("Invite not found.");

        if (invite.ExpiresAt <= DateTime.UtcNow)
        {
            throw new InvalidOperationException("Invite has expired.");
        }

        if (invite.AcceptedAt.HasValue)
        {
            throw new InvalidOperationException("Invite has already been accepted.");
        }

        if (invite.RejectedAt.HasValue)
        {
            throw new InvalidOperationException("Invite has already been rejected.");
        }

        var users = await _userRepository.ListAsync(
            u => u.Id == _authContext.UserId.Value,
            cancellationToken);

        var user = users.FirstOrDefault()
            ?? throw new InvalidOperationException("User must be registered before accepting an invite.");

        if (!string.Equals(user.Email.Value, invite.Email, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Invite email does not match authenticated user.");
        }

        var existingMemberships = await _membershipRepository.ListAsync(
            m => m.UserId == user.Id && m.WorkspaceId == invite.WorkspaceId,
            cancellationToken);

        if (existingMemberships.Count > 0)
        {
            throw new InvalidOperationException("User is already a member of this workspace.");
        }

        var membership = new UserWorkspaceMembership
        {
            UserId = user.Id,
            WorkspaceId = invite.WorkspaceId,
            Role = invite.Role
        };

        await _membershipRepository.AddAsync(membership, cancellationToken);

        invite.AcceptedAt = DateTime.UtcNow;
        _inviteRepository.Update(invite);

        await _membershipRepository.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
