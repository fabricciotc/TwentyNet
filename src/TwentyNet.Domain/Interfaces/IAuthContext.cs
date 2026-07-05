using TwentyNet.Domain.Enums;

namespace TwentyNet.Domain.Interfaces;

public interface IAuthContext
{
    Guid? UserId { get; }
    Guid? WorkspaceId { get; }
    WorkspaceRole? Role { get; }
    bool IsAuthenticated { get; }
}
