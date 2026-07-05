using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TwentyNet.Application.Companies.CreateCompany;
using TwentyNet.Application.Companies.DeleteCompany;
using TwentyNet.Application.Companies.GetCompanyById;
using TwentyNet.Application.Companies.ListCompanies;
using TwentyNet.Application.Companies.UpdateCompany;
using TwentyNet.Application.Files.AttachFileToCompany;
using TwentyNet.Application.Files.ListCompanyFiles;
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
    public async Task<ActionResult<IReadOnlyList<CompanyResponse>>> List(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ListCompaniesQuery(), cancellationToken);
        return Ok(_mapper.Map<IReadOnlyList<CompanyResponse>>(result));
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
