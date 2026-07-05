using TwentyNet.Domain.Common;
using TwentyNet.Domain.ValueObjects;

namespace TwentyNet.Domain.Entities;

public sealed class User : BaseEntity
{
    public Email Email { get; set; } = null!;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsEmailVerified { get; set; }
    public bool Disabled { get; set; }
    public ICollection<UserWorkspaceMembership> WorkspaceMemberships { get; init; } = new List<UserWorkspaceMembership>();
}
