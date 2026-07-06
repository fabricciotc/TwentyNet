using System.Text.Json;
using MediatR;

namespace TwentyNet.Application.Companies.UpdateCompanyCustomFields;

public sealed record UpdateCompanyCustomFieldsCommand(
    Guid CompanyId,
    Dictionary<string, JsonElement> CustomFields) : IRequest;
