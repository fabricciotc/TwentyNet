using TwentyNet.Domain.Common;
using TwentyNet.Domain.ValueObjects;

namespace TwentyNet.Domain.Entities;

public sealed class Person : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public Email? Email { get; set; }
    public PhoneNumber? Phone { get; set; }
    public Guid? CompanyId { get; set; }
    public Company? Company { get; set; }
    public Guid WorkspaceId { get; set; }
    public Workspace Workspace { get; set; } = null!;
}
