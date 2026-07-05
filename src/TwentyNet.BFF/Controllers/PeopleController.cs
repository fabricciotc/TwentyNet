using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TwentyNet.Application.Files.AttachFileToPerson;
using TwentyNet.Application.Files.ListPersonFiles;
using TwentyNet.Application.People.CreatePerson;
using TwentyNet.Application.People.DeletePerson;
using TwentyNet.Application.People.GetPersonById;
using TwentyNet.Application.People.ListPeople;
using TwentyNet.Application.People.UpdatePerson;
using TwentyNet.Contracts.Files;
using TwentyNet.Contracts.People;

namespace TwentyNet.BFF.Controllers;

[ApiController]
[Authorize]
[Route("api/people")]
public sealed class PeopleController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IMapper _mapper;

    public PeopleController(ISender sender, IMapper mapper)
    {
        _sender = sender;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PersonResponse>>> List(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ListPeopleQuery(), cancellationToken);
        return Ok(_mapper.Map<IReadOnlyList<PersonResponse>>(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PersonResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetPersonByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(_mapper.Map<PersonResponse>(result));
    }

    [HttpPost]
    public async Task<ActionResult<PersonResponse>> Create([FromBody] CreatePersonRequest request, CancellationToken cancellationToken)
    {
        var command = new CreatePersonCommand(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Phone,
            request.CompanyId);

        var result = await _sender.Send(command, cancellationToken);
        var response = _mapper.Map<PersonResponse>(result);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PersonResponse>> Update(Guid id, [FromBody] UpdatePersonRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdatePersonCommand(
            id,
            request.FirstName,
            request.LastName,
            request.Email,
            request.Phone,
            request.CompanyId);

        var result = await _sender.Send(command, cancellationToken);
        return Ok(_mapper.Map<PersonResponse>(result));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeletePersonCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/attachments")]
    public async Task<IActionResult> AttachFile(Guid id, [FromBody] AttachFileRequest request, CancellationToken cancellationToken)
    {
        await _sender.Send(new AttachFileToPersonCommand(id, request.FileId), cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:guid}/attachments")]
    public async Task<ActionResult<IReadOnlyList<FileResponse>>> ListFiles(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ListPersonFilesQuery(id), cancellationToken);
        return Ok(_mapper.Map<IReadOnlyList<FileResponse>>(result));
    }
}
