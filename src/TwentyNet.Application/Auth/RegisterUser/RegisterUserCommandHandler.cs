using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Enums;
using TwentyNet.Domain.Interfaces;
using TwentyNet.Domain.ValueObjects;

namespace TwentyNet.Application.Auth.RegisterUser;

public sealed class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, AuthResponse>
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Workspace> _workspaceRepository;
    private readonly IRepository<UserWorkspaceMembership> _membershipRepository;
    private readonly IRepository<RefreshToken> _refreshTokenRepository;
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;

    public RegisterUserCommandHandler(
        IRepository<User> userRepository,
        IRepository<Workspace> workspaceRepository,
        IRepository<UserWorkspaceMembership> membershipRepository,
        IRepository<RefreshToken> refreshTokenRepository,
        IPasswordService passwordService,
        ITokenService tokenService)
    {
        _userRepository = userRepository;
        _workspaceRepository = workspaceRepository;
        _membershipRepository = membershipRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordService = passwordService;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var existingUsers = await _userRepository.ListAsync(
            u => u.Email == new Domain.ValueObjects.Email(normalizedEmail),
            cancellationToken);

        if (existingUsers.Count > 0)
        {
            throw new InvalidOperationException($"A user with email '{request.Email}' already exists.");
        }

        var workspace = new Workspace { Name = request.WorkspaceName.Trim() };
        await _workspaceRepository.AddAsync(workspace, cancellationToken);
        await _workspaceRepository.SaveChangesAsync(cancellationToken);

        var user = new User
        {
            Email = new Email(request.Email),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            PasswordHash = _passwordService.Hash(request.Password),
            IsEmailVerified = false,
            Disabled = false
        };
        await _userRepository.AddAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        var membership = new UserWorkspaceMembership
        {
            UserId = user.Id,
            WorkspaceId = workspace.Id,
            Role = WorkspaceRole.Admin
        };
        await _membershipRepository.AddAsync(membership, cancellationToken);
        await _membershipRepository.SaveChangesAsync(cancellationToken);

        var accessToken = _tokenService.GenerateAccessToken(user.Id, workspace.Id, WorkspaceRole.Admin);
        var refreshTokenValue = _tokenService.GenerateRefreshToken();
        var refreshTokenHash = HashToken(refreshTokenValue);

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            WorkspaceId = workspace.Id,
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
            workspace.Id,
            WorkspaceRole.Admin);
    }

    private static string HashToken(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}
