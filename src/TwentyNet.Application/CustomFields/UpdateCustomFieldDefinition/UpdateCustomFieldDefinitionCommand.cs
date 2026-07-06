using MediatR;
using TwentyNet.Domain.Enums;

namespace TwentyNet.Application.CustomFields.UpdateCustomFieldDefinition;

public sealed record UpdateCustomFieldDefinitionCommand(
    Guid Id,
    string Label,
    CustomFieldType Type,
    string? Options,
    bool IsRequired,
    int Order) : IRequest<CustomFieldDefinitionDto>;
