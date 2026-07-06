using MediatR;
using TwentyNet.Domain.Enums;

namespace TwentyNet.Application.CustomFields.CreateCustomFieldDefinition;

public sealed record CreateCustomFieldDefinitionCommand(
    string ObjectName,
    string Name,
    string Label,
    CustomFieldType Type,
    string? Options,
    bool IsRequired,
    int Order) : IRequest<CustomFieldDefinitionDto>;
