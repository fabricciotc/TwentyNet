using TwentyNet.Domain.Enums;

namespace TwentyNet.Application.CustomFields;

public sealed record CustomFieldDefinitionDto(
    Guid Id,
    Guid WorkspaceId,
    string ObjectName,
    string Name,
    string Label,
    CustomFieldType Type,
    string? Options,
    bool IsRequired,
    int Order);
