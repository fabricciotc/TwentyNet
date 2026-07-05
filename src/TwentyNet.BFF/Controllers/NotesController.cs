using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TwentyNet.Application.Notes.CreateNote;
using TwentyNet.Application.Notes.DeleteNote;
using TwentyNet.Application.Notes.GetNoteById;
using TwentyNet.Application.Notes.ListNotes;
using TwentyNet.Application.Notes.UpdateNote;
using TwentyNet.Contracts.Notes;

namespace TwentyNet.BFF.Controllers;

[ApiController]
[Authorize(Policy = "RequireMember")]
[Route("api")]
public sealed class NotesController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IMapper _mapper;

    public NotesController(ISender sender, IMapper mapper)
    {
        _sender = sender;
        _mapper = mapper;
    }

    [HttpGet("companies/{id:guid}/notes")]
    public async Task<ActionResult<IReadOnlyList<NoteResponse>>> ListCompanyNotes(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ListNotesQuery(id, null), cancellationToken);
        return Ok(_mapper.Map<IReadOnlyList<NoteResponse>>(result));
    }

    [HttpPost("companies/{id:guid}/notes")]
    public async Task<ActionResult<NoteResponse>> CreateCompanyNote(Guid id, [FromBody] CreateNoteRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateNoteCommand(request.Title, request.Content, id, null);
        var result = await _sender.Send(command, cancellationToken);
        var response = _mapper.Map<NoteResponse>(result);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpGet("people/{id:guid}/notes")]
    public async Task<ActionResult<IReadOnlyList<NoteResponse>>> ListPersonNotes(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ListNotesQuery(null, id), cancellationToken);
        return Ok(_mapper.Map<IReadOnlyList<NoteResponse>>(result));
    }

    [HttpPost("people/{id:guid}/notes")]
    public async Task<ActionResult<NoteResponse>> CreatePersonNote(Guid id, [FromBody] CreateNoteRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateNoteCommand(request.Title, request.Content, null, id);
        var result = await _sender.Send(command, cancellationToken);
        var response = _mapper.Map<NoteResponse>(result);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpGet("notes/{id:guid}")]
    public async Task<ActionResult<NoteResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetNoteByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(_mapper.Map<NoteResponse>(result));
    }

    [HttpPut("notes/{id:guid}")]
    public async Task<ActionResult<NoteResponse>> Update(Guid id, [FromBody] UpdateNoteRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateNoteCommand(id, request.Title, request.Content);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(_mapper.Map<NoteResponse>(result));
    }

    [HttpDelete("notes/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteNoteCommand(id), cancellationToken);
        return NoContent();
    }
}
