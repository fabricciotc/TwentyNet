using TwentyNet.Domain.Common;
using TwentyNet.Domain.ValueObjects;

namespace TwentyNet.Domain.Entities;

public sealed class User : BaseEntity
{
    public Email Email { get; set; } = null!;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public Guid WorkspaceId { get; set; }
    public Workspace Workspace { get; set; } = null!;
}
