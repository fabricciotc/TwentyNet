namespace TwentyNet.Domain.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(Guid userId, Guid workspaceId);
    string GenerateRefreshToken();
}
