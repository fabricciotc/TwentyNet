using System.Security.Cryptography;
using System.Text;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Auth.LogoutUser;

public sealed class LogoutUserCommandHandler : IRequestHandler<LogoutUserCommand, Unit>
{
    private readonly IRepository<RefreshToken> _refreshTokenRepository;

    public LogoutUserCommandHandler(IRepository<RefreshToken> refreshTokenRepository)
    {
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task<Unit> Handle(LogoutUserCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = HashToken(request.RefreshToken);

        var tokens = await _refreshTokenRepository.ListAsync(
            t => t.TokenHash == tokenHash,
            cancellationToken);

        var storedToken = tokens.FirstOrDefault();

        if (storedToken is not null && !storedToken.RevokedAt.HasValue)
        {
            storedToken.RevokedAt = DateTime.UtcNow;
            _refreshTokenRepository.Update(storedToken);
            await _refreshTokenRepository.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }

    private static string HashToken(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}
