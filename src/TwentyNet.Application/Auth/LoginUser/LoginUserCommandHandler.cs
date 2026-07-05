using System.Security.Cryptography;
using System.Text;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Auth.LoginUser;

public sealed class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, AuthResponse>
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<UserWorkspaceMembership> _membershipRepository;
    private readonly IRepository<RefreshToken> _refreshTokenRepository;
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;

    public LoginUserCommandHandler(
        IRepository<User> userRepository,
        IRepository<UserWorkspaceMembership> membershipRepository,
        IRepository<RefreshToken> refreshTokenRepository,
        IPasswordService passwordService,
        ITokenService tokenService)
    {
        _userRepository = userRepository;
        _membershipRepository = membershipRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordService = passwordService;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.ListAsync(
            u => u.Email.Value == request.Email.Trim().ToLowerInvariant(),
            cancellationToken);

        var user = users.FirstOrDefault();

        if (user is null || !_passwordService.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        if (user.Disabled)
        {
            throw new UnauthorizedAccessException("User account is disabled.");
        }

        var membership = await _membershipRepository.ListAsync(
            m => m.UserId == user.Id && m.WorkspaceId == request.WorkspaceId,
            cancellationToken);

        if (membership.Count == 0)
        {
            throw new UnauthorizedAccessException("User is not a member of the specified workspace.");
        }

        var accessToken = _tokenService.GenerateAccessToken(user.Id, request.WorkspaceId);
        var refreshTokenValue = _tokenService.GenerateRefreshToken();
        var refreshTokenHash = HashToken(refreshTokenValue);

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            WorkspaceId = request.WorkspaceId,
            TokenHash = refreshTokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

        return new AuthResponse(
            accessToken,
            refreshTokenValue,
            3600,
            user.Id,
            request.WorkspaceId);
    }

    private static string HashToken(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}
