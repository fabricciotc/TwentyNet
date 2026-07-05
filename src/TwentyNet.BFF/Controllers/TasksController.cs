using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TwentyNet.Application.Tasks.CompleteTask;
using TwentyNet.Application.Tasks.CreateTask;
using TwentyNet.Application.Tasks.DeleteTask;
using TwentyNet.Application.Tasks.GetTaskById;
using TwentyNet.Application.Tasks.ListTasks;
using TwentyNet.Application.Tasks.UpdateTask;
using TwentyNet.Contracts.Tasks;

namespace TwentyNet.BFF.Controllers;

[ApiController]
[Authorize(Policy = "RequireMember")]
[Route("api")]
public sealed class TasksController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IMapper _mapper;

    public TasksController(ISender sender, IMapper mapper)
    {
        _sender = sender;
        _mapper = mapper;
    }

    [HttpGet("companies/{id:guid}/tasks")]
    public async Task<ActionResult<IReadOnlyList<TaskResponse>>> ListCompanyTasks(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ListTasksQuery(id, null), cancellationToken);
        return Ok(_mapper.Map<IReadOnlyList<TaskResponse>>(result));
    }

    [HttpPost("companies/{id:guid}/tasks")]
    public async Task<ActionResult<TaskResponse>> CreateCompanyTask(Guid id, [FromBody] CreateTaskRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateTaskCommand(request.Title, request.AssignedToUserId, request.DueDate, id, null);
        var result = await _sender.Send(command, cancellationToken);
        var response = _mapper.Map<TaskResponse>(result);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpGet("people/{id:guid}/tasks")]
    public async Task<ActionResult<IReadOnlyList<TaskResponse>>> ListPersonTasks(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ListTasksQuery(null, id), cancellationToken);
        return Ok(_mapper.Map<IReadOnlyList<TaskResponse>>(result));
    }

    [HttpPost("people/{id:guid}/tasks")]
    public async Task<ActionResult<TaskResponse>> CreatePersonTask(Guid id, [FromBody] CreateTaskRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateTaskCommand(request.Title, request.AssignedToUserId, request.DueDate, null, id);
        var result = await _sender.Send(command, cancellationToken);
        var response = _mapper.Map<TaskResponse>(result);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpGet("tasks/{id:guid}")]
    public async Task<ActionResult<TaskResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetTaskByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(_mapper.Map<TaskResponse>(result));
    }

    [HttpPost("tasks/{id:guid}/complete")]
    public async Task<ActionResult<TaskResponse>> Complete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CompleteTaskCommand(id), cancellationToken);
        return Ok(_mapper.Map<TaskResponse>(result));
    }

    [HttpPut("tasks/{id:guid}")]
    public async Task<ActionResult<TaskResponse>> Update(Guid id, [FromBody] UpdateTaskRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateTaskCommand(id, request.Title, request.Status, request.AssignedToUserId, request.DueDate);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(_mapper.Map<TaskResponse>(result));
    }

    [HttpDelete("tasks/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteTaskCommand(id), cancellationToken);
        return NoContent();
    }
}
