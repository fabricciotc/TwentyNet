using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TwentyNet.Application.Common;
using TwentyNet.Application.Views.CreateView;
using TwentyNet.Application.Views.DeleteView;
using TwentyNet.Application.Views.GetViewById;
using TwentyNet.Application.Views.ListViews;
using TwentyNet.Application.Views.UpdateView;
using TwentyNet.Contracts.Common;
using TwentyNet.Contracts.Views;

namespace TwentyNet.BFF.Controllers;

[ApiController]
[Authorize(Policy = "RequireMember")]
[Route("api/views")]
public sealed class ViewsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IMapper _mapper;

    public ViewsController(ISender sender, IMapper mapper)
    {
        _sender = sender;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ViewResponse>>> List(
        [FromQuery] string? objectName,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ListViewsQuery(objectName), cancellationToken);
        return Ok(_mapper.Map<IReadOnlyList<ViewResponse>>(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ViewResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetViewByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(_mapper.Map<ViewResponse>(result));
    }

    [HttpPost]
    public async Task<ActionResult<ViewResponse>> Create(
        [FromBody] CreateViewRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateViewCommand(
            request.ObjectName,
            request.Name,
            request.IsDefault,
            request.Filters.Select(f => new FilterInput(f.Field, f.Operator, f.Value)).ToList(),
            request.Sorts.Select(s => new SortInput(s.Field, s.Direction)).ToList());

        var result = await _sender.Send(command, cancellationToken);
        var response = _mapper.Map<ViewResponse>(result);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ViewResponse>> Update(
        Guid id,
        [FromBody] UpdateViewRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateViewCommand(
            id,
            request.ObjectName,
            request.Name,
            request.IsDefault,
            request.Filters.Select(f => new FilterInput(f.Field, f.Operator, f.Value)).ToList(),
            request.Sorts.Select(s => new SortInput(s.Field, s.Direction)).ToList());

        var result = await _sender.Send(command, cancellationToken);
        return Ok(_mapper.Map<ViewResponse>(result));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteViewCommand(id), cancellationToken);
        return NoContent();
    }
}
