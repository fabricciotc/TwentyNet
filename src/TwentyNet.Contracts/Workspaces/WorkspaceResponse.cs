namespace TwentyNet.Contracts.Workspaces;

public sealed record WorkspaceResponse(
    Guid Id,
    string Name,
    string Role,
    DateTime CreatedAt);
