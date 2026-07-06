using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TwentyNet.Application.Workflows.CreateWorkflow;
using TwentyNet.Application.Workflows.DeleteWorkflow;
using TwentyNet.Application.Workflows.GetWorkflowById;
using TwentyNet.Application.Workflows.ListWorkflows;
using TwentyNet.Application.Workflows.UpdateWorkflow;
using TwentyNet.Contracts.Workflows;
using DomainEnums = TwentyNet.Domain.Enums;
using WorkflowActionConfig = TwentyNet.Domain.Workflows.WorkflowActionConfig;

namespace TwentyNet.BFF.Controllers;

[ApiController]
[Authorize(Policy = "RequireMember")]
[Route("api/workflows")]
public sealed class WorkflowController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IMapper _mapper;

    public WorkflowController(ISender sender, IMapper mapper)
    {
        _sender = sender;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<WorkflowResponse>>> List(
        [FromQuery] bool? isActive,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ListWorkflowsQuery(isActive), cancellationToken);
        return Ok(_mapper.Map<IReadOnlyList<WorkflowResponse>>(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<WorkflowResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetWorkflowByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(_mapper.Map<WorkflowResponse>(result));
    }

    [HttpPost]
    public async Task<ActionResult<WorkflowResponse>> Create(
        [FromBody] CreateWorkflowRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateWorkflowCommand(
            request.Name,
            MapTriggerType(request.TriggerType),
            request.TriggerObjectName,
            request.TriggerFieldName,
            _mapper.Map<IReadOnlyList<WorkflowActionConfig>>(request.Actions));

        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, _mapper.Map<WorkflowResponse>(result));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<WorkflowResponse>> Update(
        Guid id,
        [FromBody] UpdateWorkflowRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateWorkflowCommand(
            id,
            request.Name,
            request.IsActive,
            MapTriggerType(request.TriggerType),
            request.TriggerObjectName,
            request.TriggerFieldName,
            _mapper.Map<IReadOnlyList<WorkflowActionConfig>>(request.Actions));

        var result = await _sender.Send(command, cancellationToken);
        return Ok(_mapper.Map<WorkflowResponse>(result));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteWorkflowCommand(id), cancellationToken);
        return NoContent();
    }

    private static DomainEnums.WorkflowTriggerType MapTriggerType(string triggerType)
    {
        if (!Enum.TryParse<DomainEnums.WorkflowTriggerType>(triggerType, true, out var result))
        {
            throw new ArgumentException($"Invalid trigger type '{triggerType}'.", nameof(triggerType));
        }

        return result;
    }
}
