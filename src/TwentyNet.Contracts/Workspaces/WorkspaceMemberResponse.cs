namespace TwentyNet.Contracts.Workspaces;

public sealed record WorkspaceMemberResponse(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string Role);
