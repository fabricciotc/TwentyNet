using TwentyNet.Domain.Enums;

namespace TwentyNet.Application.Workspaces;

public sealed record WorkspaceDto(
    Guid Id,
    string Name,
    WorkspaceRole Role,
    DateTime CreatedAt);
