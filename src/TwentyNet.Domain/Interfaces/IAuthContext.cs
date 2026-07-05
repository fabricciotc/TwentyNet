namespace TwentyNet.Domain.Interfaces;

public interface IAuthContext
{
    Guid? UserId { get; }
    Guid? WorkspaceId { get; }
    bool IsAuthenticated { get; }
}
