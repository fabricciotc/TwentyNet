using System.Text.Json;
using MediatR;

namespace TwentyNet.Application.People.UpdatePersonCustomFields;

public sealed record UpdatePersonCustomFieldsCommand(
    Guid PersonId,
    Dictionary<string, JsonElement> CustomFields) : IRequest;
