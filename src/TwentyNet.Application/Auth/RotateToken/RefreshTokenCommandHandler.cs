using System.Security.Cryptography;
using System.Text;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Enums;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Auth.RotateToken;

public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly IRepository<RefreshToken> _refreshTokenRepository;
    private readonly IRepository<UserWorkspaceMembership> _membershipRepository;
    private readonly ITokenService _tokenService;

    public RefreshTokenCommandHandler(
        IRepository<RefreshToken> refreshTokenRepository,
        IRepository<UserWorkspaceMembership> membershipRepository,
        ITokenService tokenService)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _membershipRepository = membershipRepository;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = HashToken(request.RefreshToken);

        var tokens = await _refreshTokenRepository.ListAsync(
            t => t.TokenHash == tokenHash,
            cancellationToken);

        var storedToken = tokens.FirstOrDefault();

        if (storedToken is null)
        {
            throw new UnauthorizedAccessException("Invalid refresh token.");
        }

        if (storedToken.RevokedAt.HasValue || storedToken.ExpiresAt <= DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Refresh token has been revoked or expired.");
        }

        var memberships = await _membershipRepository.ListAsync(
            m => m.UserId == storedToken.UserId && m.WorkspaceId == storedToken.WorkspaceId,
            cancellationToken);

        var membership = memberships.FirstOrDefault();
        if (membership is null)
        {
            throw new UnauthorizedAccessException("User is not a member of the specified workspace.");
        }

        storedToken.RevokedAt = DateTime.UtcNow;
        _refreshTokenRepository.Update(storedToken);
        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

        var accessToken = _tokenService.GenerateAccessToken(storedToken.UserId, storedToken.WorkspaceId, membership.Role);
        var refreshTokenValue = _tokenService.GenerateRefreshToken();
        var newRefreshTokenHash = HashToken(refreshTokenValue);

        var newRefreshToken = new RefreshToken
        {
            UserId = storedToken.UserId,
            WorkspaceId = storedToken.WorkspaceId,
            TokenHash = newRefreshTokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
        await _refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);
        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

        return new AuthResponse(
            accessToken,
            refreshTokenValue,
            3600,
            storedToken.UserId,
            storedToken.WorkspaceId,
            membership.Role);
    }

    private static string HashToken(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}
