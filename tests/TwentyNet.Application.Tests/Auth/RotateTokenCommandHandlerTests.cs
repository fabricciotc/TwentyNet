using System.Security.Cryptography;
using System.Text;
using NSubstitute;
using TwentyNet.Application.Auth.RotateToken;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;
using TwentyNet.Persistence.Repositories;

namespace TwentyNet.Application.Tests.Auth;

public sealed class RefreshTokenCommandHandlerTests : TestBase
{
    [Fact]
    public async Task Handle_ShouldRotateRefreshToken_AndReturnNewTokens()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var userId = Guid.NewGuid();
        var workspaceId = Guid.NewGuid();
        var refreshTokenValue = "old-refresh-token";
        var oldHash = HashToken(refreshTokenValue);

        await context.RefreshTokens.AddAsync(new RefreshToken
        {
            UserId = userId,
            WorkspaceId = workspaceId,
            TokenHash = oldHash,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        });
        await context.SaveChangesAsync();

        var refreshTokenRepository = new EfRepository<RefreshToken>(context);
        var tokenService = Substitute.For<ITokenService>();
        tokenService.GenerateAccessToken(userId, workspaceId).Returns("new-access-token");
        tokenService.GenerateRefreshToken().Returns("new-refresh-token");

        var handler = new RefreshTokenCommandHandler(refreshTokenRepository, tokenService);

        var command = new RefreshTokenCommand(refreshTokenValue);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("new-access-token", result.AccessToken);
        Assert.Equal("new-refresh-token", result.RefreshToken);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(workspaceId, result.WorkspaceId);

        var oldToken = context.RefreshTokens.First(t => t.TokenHash == oldHash);
        Assert.NotNull(oldToken.RevokedAt);

        var newHash = HashToken("new-refresh-token");
        Assert.NotNull(context.RefreshTokens.FirstOrDefault(t => t.TokenHash == newHash));
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenTokenIsExpired()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var refreshTokenValue = "expired-token";
        var hash = HashToken(refreshTokenValue);

        await context.RefreshTokens.AddAsync(new RefreshToken
        {
            UserId = Guid.NewGuid(),
            WorkspaceId = Guid.NewGuid(),
            TokenHash = hash,
            ExpiresAt = DateTime.UtcNow.AddDays(-1)
        });
        await context.SaveChangesAsync();

        var refreshTokenRepository = new EfRepository<RefreshToken>(context);
        var tokenService = Substitute.For<ITokenService>();
        var handler = new RefreshTokenCommandHandler(refreshTokenRepository, tokenService);

        var command = new RefreshTokenCommand(refreshTokenValue);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(command, CancellationToken.None));
    }

    private static string HashToken(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}
