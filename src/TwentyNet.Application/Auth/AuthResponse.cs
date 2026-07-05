namespace TwentyNet.Application.Auth;

public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    Guid UserId,
    Guid WorkspaceId);
