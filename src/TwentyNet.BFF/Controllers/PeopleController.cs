using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TwentyNet.Application.Common;
using TwentyNet.Application.Files.AttachFileToPerson;
using TwentyNet.Application.Files.ListPersonFiles;
using TwentyNet.Application.People.CreatePerson;
using TwentyNet.Application.People.DeletePerson;
using TwentyNet.Application.People.GetPersonById;
using TwentyNet.Application.People.ListPeople;
using TwentyNet.Application.People.UpdatePerson;
using TwentyNet.Contracts.Common;
using TwentyNet.Contracts.Files;
using TwentyNet.Contracts.People;

namespace TwentyNet.BFF.Controllers;

[ApiController]
[Authorize(Policy = "RequireMember")]
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
    public async Task<ActionResult<PagedResponse<PersonResponse>>> List(
        [FromQuery] Guid? viewId,
        [FromQuery] string? search,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        CancellationToken cancellationToken = default)
    {
        var query = new ListPeopleQuery(viewId, search, null, null, skip, take);
        var result = await _sender.Send(query, cancellationToken);
        return Ok(_mapper.Map<PagedResponse<PersonResponse>>(result));
    }

    [HttpPost("search")]
    public async Task<ActionResult<PagedResponse<PersonResponse>>> Search(
        [FromBody] PersonSearchRequest request,
        CancellationToken cancellationToken)
    {
        var query = new ListPeopleQuery(
            request.ViewId,
            request.Search,
            request.Filters?.Select(f => new FilterInput(f.Field, f.Operator, f.Value)).ToList(),
            request.Sorts?.Select(s => new SortInput(s.Field, s.Direction)).ToList(),
            request.Skip,
            request.Take);

        var result = await _sender.Send(query, cancellationToken);
        return Ok(_mapper.Map<PagedResponse<PersonResponse>>(result));
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
