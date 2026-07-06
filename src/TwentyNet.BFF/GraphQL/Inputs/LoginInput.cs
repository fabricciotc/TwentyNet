namespace TwentyNet.BFF.GraphQL.Inputs;

public sealed record LoginInput(
    string Email,
    string Password,
    Guid WorkspaceId);
