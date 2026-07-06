using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TwentyNet.Application.CustomFields;
using TwentyNet.Application.CustomFields.CreateCustomFieldDefinition;
using TwentyNet.Application.CustomFields.DeleteCustomFieldDefinition;
using TwentyNet.Application.CustomFields.ListCustomFieldDefinitions;
using TwentyNet.Application.CustomFields.UpdateCustomFieldDefinition;
using TwentyNet.Domain.Enums;

namespace TwentyNet.BFF.Controllers;

[ApiController]
[Route("api/custom-fields")]
[Authorize(Policy = "RequireMember")]
public sealed class CustomFieldsController : ControllerBase
{
    private readonly ISender _sender;

    public CustomFieldsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CustomFieldDefinitionDto>>> List([FromQuery] string objectName, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ListCustomFieldDefinitionsQuery(objectName), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<ActionResult<CustomFieldDefinitionDto>> Create([FromBody] CreateCustomFieldDefinitionRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateCustomFieldDefinitionCommand(
            request.ObjectName,
            request.Name,
            request.Label,
            request.Type,
            request.Options,
            request.IsRequired,
            request.Order);

        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(List), new { objectName = result.ObjectName }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<ActionResult<CustomFieldDefinitionDto>> Update(Guid id, [FromBody] UpdateCustomFieldDefinitionRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateCustomFieldDefinitionCommand(
            id,
            request.Label,
            request.Type,
            request.Options,
            request.IsRequired,
            request.Order);

        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteCustomFieldDefinitionCommand(id), cancellationToken);
        return NoContent();
    }
}

public sealed record CreateCustomFieldDefinitionRequest(
    string ObjectName,
    string Name,
    string Label,
    CustomFieldType Type,
    string? Options,
    bool IsRequired,
    int Order);

public sealed record UpdateCustomFieldDefinitionRequest(
    string Label,
    CustomFieldType Type,
    string? Options,
    bool IsRequired,
    int Order);
