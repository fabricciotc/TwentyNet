using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Workspaces.RejectInvite;

public sealed class RejectInviteCommandHandler : IRequestHandler<RejectInviteCommand, Unit>
{
    private readonly IRepository<WorkspaceInvite> _inviteRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IAuthContext _authContext;

    public RejectInviteCommandHandler(
        IRepository<WorkspaceInvite> inviteRepository,
        IRepository<User> userRepository,
        IAuthContext authContext)
    {
        _inviteRepository = inviteRepository;
        _userRepository = userRepository;
        _authContext = authContext;
    }

    public async Task<Unit> Handle(RejectInviteCommand request, CancellationToken cancellationToken)
    {
        if (!_authContext.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User must be authenticated to reject an invite.");
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
            ?? throw new InvalidOperationException("User not found.");

        if (!string.Equals(user.Email.Value, invite.Email, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Invite email does not match authenticated user.");
        }

        invite.RejectedAt = DateTime.UtcNow;
        _inviteRepository.Update(invite);
        await _inviteRepository.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
