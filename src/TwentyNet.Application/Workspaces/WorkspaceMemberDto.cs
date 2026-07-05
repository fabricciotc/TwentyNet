using TwentyNet.Domain.Enums;

namespace TwentyNet.Application.Workspaces;

public sealed record WorkspaceMemberDto(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    WorkspaceRole Role);
