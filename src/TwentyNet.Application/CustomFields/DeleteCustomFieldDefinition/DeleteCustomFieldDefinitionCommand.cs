using MediatR;

namespace TwentyNet.Application.CustomFields.DeleteCustomFieldDefinition;

public sealed record DeleteCustomFieldDefinitionCommand(Guid Id) : IRequest;
