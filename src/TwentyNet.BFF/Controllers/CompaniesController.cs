using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TwentyNet.Application.Common;
using TwentyNet.Application.Companies.CreateCompany;
using TwentyNet.Application.Companies.DeleteCompany;
using TwentyNet.Application.Companies.GetCompanyById;
using TwentyNet.Application.Companies.ListCompanies;
using TwentyNet.Application.Companies.UpdateCompany;
using TwentyNet.Application.Files.AttachFileToCompany;
using TwentyNet.Application.Files.ListCompanyFiles;
using TwentyNet.Contracts.Common;
using TwentyNet.Contracts.Companies;
using TwentyNet.Contracts.Files;

namespace TwentyNet.BFF.Controllers;

[ApiController]
[Authorize(Policy = "RequireMember")]
[Route("api/companies")]
public sealed class CompaniesController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IMapper _mapper;

    public CompaniesController(ISender sender, IMapper mapper)
    {
        _sender = sender;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<CompanyResponse>>> List(
        [FromQuery] Guid? viewId,
        [FromQuery] string? search,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        CancellationToken cancellationToken = default)
    {
        var query = new ListCompaniesQuery(viewId, search, null, null, skip, take);
        var result = await _sender.Send(query, cancellationToken);
        return Ok(_mapper.Map<PagedResponse<CompanyResponse>>(result));
    }

    [HttpPost("search")]
    public async Task<ActionResult<PagedResponse<CompanyResponse>>> Search(
        [FromBody] CompanySearchRequest request,
        CancellationToken cancellationToken)
    {
        var query = new ListCompaniesQuery(
            request.ViewId,
            request.Search,
            request.Filters?.Select(f => new FilterInput(f.Field, f.Operator, f.Value)).ToList(),
            request.Sorts?.Select(s => new SortInput(s.Field, s.Direction)).ToList(),
            request.Skip,
            request.Take);

        var result = await _sender.Send(query, cancellationToken);
        return Ok(_mapper.Map<PagedResponse<CompanyResponse>>(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CompanyResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetCompanyByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(_mapper.Map<CompanyResponse>(result));
    }

    [HttpPost]
    public async Task<ActionResult<CompanyResponse>> Create([FromBody] CreateCompanyRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateCompanyCommand(request.Name, request.DomainName, request.Address);
        var result = await _sender.Send(command, cancellationToken);
        var response = _mapper.Map<CompanyResponse>(result);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CompanyResponse>> Update(Guid id, [FromBody] UpdateCompanyRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateCompanyCommand(id, request.Name, request.DomainName, request.Address);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(_mapper.Map<CompanyResponse>(result));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteCompanyCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/attachments")]
    public async Task<IActionResult> AttachFile(Guid id, [FromBody] AttachFileRequest request, CancellationToken cancellationToken)
    {
        await _sender.Send(new AttachFileToCompanyCommand(id, request.FileId), cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:guid}/attachments")]
    public async Task<ActionResult<IReadOnlyList<FileResponse>>> ListFiles(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ListCompanyFilesQuery(id), cancellationToken);
        return Ok(_mapper.Map<IReadOnlyList<FileResponse>>(result));
    }
}
