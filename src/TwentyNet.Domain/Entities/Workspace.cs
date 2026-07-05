using TwentyNet.Domain.Common;

namespace TwentyNet.Domain.Entities;

public sealed class Workspace : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public ICollection<UserWorkspaceMembership> UserMemberships { get; init; } = new List<UserWorkspaceMembership>();
    public ICollection<Company> Companies { get; init; } = new List<Company>();
    public ICollection<Person> People { get; init; } = new List<Person>();
}
