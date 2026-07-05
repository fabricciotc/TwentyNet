using TwentyNet.Domain.Common;

namespace TwentyNet.Domain.Entities;

public sealed class Company : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? DomainName { get; set; }
    public string? Address { get; set; }
    public Guid WorkspaceId { get; set; }
    public Workspace Workspace { get; set; } = null!;
    public ICollection<Person> People { get; init; } = new List<Person>();
}
