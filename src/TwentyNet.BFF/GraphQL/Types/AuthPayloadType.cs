namespace TwentyNet.BFF.GraphQL.Types;

public sealed record AuthPayloadType(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    Guid UserId,
    Guid WorkspaceId,
    string Role);
