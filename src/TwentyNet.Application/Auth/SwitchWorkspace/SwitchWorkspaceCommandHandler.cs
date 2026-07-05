using System.Security.Cryptography;
using System.Text;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Auth.SwitchWorkspace;

public sealed class SwitchWorkspaceCommandHandler : IRequestHandler<SwitchWorkspaceCommand, AuthResponse>
{
    private readonly IRepository<UserWorkspaceMembership> _membershipRepository;
    private readonly IRepository<RefreshToken> _refreshTokenRepository;
    private readonly ITokenService _tokenService;
    private readonly IAuthContext _authContext;

    public SwitchWorkspaceCommandHandler(
        IRepository<UserWorkspaceMembership> membershipRepository,
        IRepository<RefreshToken> refreshTokenRepository,
        ITokenService tokenService,
        IAuthContext authContext)
    {
        _membershipRepository = membershipRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _tokenService = tokenService;
        _authContext = authContext;
    }

    public async Task<AuthResponse> Handle(SwitchWorkspaceCommand request, CancellationToken cancellationToken)
    {
        if (!_authContext.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User is required.");
        }

        var memberships = await _membershipRepository.ListAsync(
            m => m.UserId == _authContext.UserId.Value && m.WorkspaceId == request.WorkspaceId,
            cancellationToken);

        var membership = memberships.FirstOrDefault();

        if (membership is null)
        {
            throw new UnauthorizedAccessException("User is not a member of the specified workspace.");
        }

        var accessToken = _tokenService.GenerateAccessToken(membership.UserId, membership.WorkspaceId, membership.Role);
        var refreshTokenValue = _tokenService.GenerateRefreshToken();
        var refreshTokenHash = HashToken(refreshTokenValue);

        var refreshToken = new RefreshToken
        {
            UserId = membership.UserId,
            WorkspaceId = membership.WorkspaceId,
            TokenHash = refreshTokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

        return new AuthResponse(
            accessToken,
            refreshTokenValue,
            3600,
            membership.UserId,
            membership.WorkspaceId,
            membership.Role);
    }

    private static string HashToken(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}
