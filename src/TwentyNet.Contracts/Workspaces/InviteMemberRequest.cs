namespace TwentyNet.Contracts.Workspaces;

public sealed record InviteMemberRequest(
    string Email,
    string Role);
