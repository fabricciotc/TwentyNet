using TwentyNet.Domain.Enums;

namespace TwentyNet.Domain.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(Guid userId, Guid workspaceId, WorkspaceRole role);
    string GenerateRefreshToken();
}
