using MediatR;
using Microsoft.Extensions.Logging;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Enums;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Workspaces.InviteMember;

public sealed class InviteMemberCommandHandler : IRequestHandler<InviteMemberCommand, WorkspaceInviteDto>
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<UserWorkspaceMembership> _membershipRepository;
    private readonly IRepository<WorkspaceInvite> _inviteRepository;
    private readonly IAuthContext _authContext;
    private readonly ILogger<InviteMemberCommandHandler> _logger;

    public InviteMemberCommandHandler(
        IRepository<User> userRepository,
        IRepository<UserWorkspaceMembership> membershipRepository,
        IRepository<WorkspaceInvite> inviteRepository,
        IAuthContext authContext,
        ILogger<InviteMemberCommandHandler> logger)
    {
        _userRepository = userRepository;
        _membershipRepository = membershipRepository;
        _inviteRepository = inviteRepository;
        _authContext = authContext;
        _logger = logger;
    }

    public async Task<WorkspaceInviteDto> Handle(InviteMemberCommand request, CancellationToken cancellationToken)
    {
        if (_authContext.Role != WorkspaceRole.Admin)
        {
            throw new UnauthorizedAccessException("Only workspace admins can invite members.");
        }

        if (_authContext.WorkspaceId != request.WorkspaceId)
        {
            throw new UnauthorizedAccessException("Cannot invite members to a different workspace.");
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var users = await _userRepository.ListAsync(
            u => u.Email == new Domain.ValueObjects.Email(normalizedEmail),
            cancellationToken);
        var invitedUser = users.FirstOrDefault();

        if (invitedUser is not null)
        {
            var existingMemberships = await _membershipRepository.ListAsync(
                m => m.WorkspaceId == request.WorkspaceId && m.UserId == invitedUser.Id,
                cancellationToken);

            if (existingMemberships.Count > 0)
            {
                throw new InvalidOperationException("User is already a member of this workspace.");
            }
        }

        var pendingInvites = await _inviteRepository.ListAsync(
            i =>
                i.WorkspaceId == request.WorkspaceId &&
                i.Email == normalizedEmail &&
                i.ExpiresAt > DateTime.UtcNow &&
                !i.AcceptedAt.HasValue &&
                !i.RejectedAt.HasValue,
            cancellationToken);

        if (pendingInvites.Count > 0)
        {
            throw new InvalidOperationException("A pending invite already exists for this email.");
        }

        var invite = new WorkspaceInvite
        {
            WorkspaceId = request.WorkspaceId,
            Email = normalizedEmail,
            Role = request.Role,
            Token = Guid.NewGuid().ToString("N"),
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        await _inviteRepository.AddAsync(invite, cancellationToken);
        await _inviteRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Workspace invite created for {Email} to workspace {WorkspaceId} with role {Role}. Email would be sent here in a production environment.",
            invite.Email,
            invite.WorkspaceId,
            invite.Role);

        return new WorkspaceInviteDto(
            invite.Id,
            invite.WorkspaceId,
            invite.Email,
            invite.Role,
            invite.Token,
            invite.ExpiresAt,
            invite.AcceptedAt,
            invite.RejectedAt);
    }
}
