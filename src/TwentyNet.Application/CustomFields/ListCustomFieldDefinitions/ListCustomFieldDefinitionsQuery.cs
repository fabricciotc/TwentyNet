using MediatR;

namespace TwentyNet.Application.CustomFields.ListCustomFieldDefinitions;

public sealed record ListCustomFieldDefinitionsQuery(string ObjectName) : IRequest<IReadOnlyList<CustomFieldDefinitionDto>>;
