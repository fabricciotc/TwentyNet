using TwentyNet.Domain.Common;
using TwentyNet.Domain.Enums;

namespace TwentyNet.Domain.Entities;

public sealed class File : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long Size { get; set; }
    public FileFolder Folder { get; set; }
    public string StorageKey { get; set; } = string.Empty;
    public FileStatus Status { get; set; } = FileStatus.Pending;

    public Guid WorkspaceId { get; set; }
    public Workspace Workspace { get; set; } = null!;

    public Guid? PersonId { get; set; }
    public Person? Person { get; set; }

    public Guid? CompanyId { get; set; }
    public Company? Company { get; set; }

    public DateTime? DeletedAt { get; set; }
}
