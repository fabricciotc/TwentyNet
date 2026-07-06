using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TwentyNet.Application.RecordRelations;
using TwentyNet.Application.RecordRelations.CreateRecordRelation;
using TwentyNet.Application.RecordRelations.DeleteRecordRelation;
using TwentyNet.Application.RecordRelations.ListRecordRelations;

namespace TwentyNet.BFF.Controllers;

[ApiController]
[Route("api/relations")]
[Authorize(Policy = "RequireMember")]
public sealed class RelationsController : ControllerBase
{
    private readonly ISender _sender;

    public RelationsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RecordRelationDto>>> List(
        [FromQuery] string objectName,
        [FromQuery] Guid recordId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ListRecordRelationsQuery(objectName, recordId), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<RecordRelationDto>> Create([FromBody] CreateRecordRelationRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateRecordRelationCommand(
            request.SourceObjectName,
            request.SourceRecordId,
            request.TargetObjectName,
            request.TargetRecordId,
            request.RelationType);

        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(List), new { objectName = result.SourceObjectName, recordId = result.SourceRecordId }, result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteRecordRelationCommand(id), cancellationToken);
        return NoContent();
    }
}

public sealed record CreateRecordRelationRequest(
    string SourceObjectName,
    Guid SourceRecordId,
    string TargetObjectName,
    Guid TargetRecordId,
    string RelationType);
