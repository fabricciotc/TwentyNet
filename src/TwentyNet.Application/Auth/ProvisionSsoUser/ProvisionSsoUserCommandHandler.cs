using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Enums;
using TwentyNet.Domain.Interfaces;
using TwentyNet.Domain.ValueObjects;

namespace TwentyNet.Application.Auth.ProvisionSsoUser;

public sealed class ProvisionSsoUserCommandHandler : IRequestHandler<ProvisionSsoUserCommand, AuthResponse>
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<UserWorkspaceMembership> _membershipRepository;
    private readonly IRepository<Workspace> _workspaceRepository;
    private readonly ITokenService _tokenService;

    public ProvisionSsoUserCommandHandler(
        IRepository<User> userRepository,
        IRepository<UserWorkspaceMembership> membershipRepository,
        IRepository<Workspace> workspaceRepository,
        ITokenService tokenService)
    {
        _userRepository = userRepository;
        _membershipRepository = membershipRepository;
        _workspaceRepository = workspaceRepository;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> Handle(ProvisionSsoUserCommand request, CancellationToken cancellationToken)
    {
        var email = new Email(request.Email);

        var users = await _userRepository.ListAsync(
            u => u.Email != null && u.Email.Value == email.Value,
            cancellationToken);
        var user = users.FirstOrDefault();

        if (user is null)
        {
            user = new User
            {
                Email = email,
                FirstName = request.FirstName ?? string.Empty,
                LastName = request.LastName ?? string.Empty,
                PasswordHash = string.Empty // SSO users have no local password
            };

            await _userRepository.AddAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);
        }

        var workspace = await _workspaceRepository.GetByIdAsync(request.WorkspaceId, cancellationToken)
            ?? throw new KeyNotFoundException($"Workspace {request.WorkspaceId} not found.");

        var memberships = await _membershipRepository.ListAsync(
            m => m.UserId == user.Id && m.WorkspaceId == workspace.Id,
            cancellationToken);

        if (memberships.Count == 0)
        {
            var membership = new UserWorkspaceMembership
            {
                UserId = user.Id,
                WorkspaceId = workspace.Id,
                Role = request.Role
            };

            await _membershipRepository.AddAsync(membership, cancellationToken);
            await _membershipRepository.SaveChangesAsync(cancellationToken);
        }

        var accessToken = _tokenService.GenerateAccessToken(user.Id, workspace.Id, request.Role);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var expiresIn = _tokenService.AccessTokenExpirationMinutes * 60;

        return new AuthResponse(accessToken, refreshToken, expiresIn, user.Id, workspace.Id, request.Role);
    }
}
