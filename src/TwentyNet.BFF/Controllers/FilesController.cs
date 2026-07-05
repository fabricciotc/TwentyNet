using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TwentyNet.Application.Files.CompleteFileUpload;
using TwentyNet.Application.Files.CreateFileUpload;
using TwentyNet.Application.Files.DeleteFile;
using TwentyNet.Application.Files.GetFileDownloadUrl;
using TwentyNet.Application.Files.UploadFileContent;
using TwentyNet.Contracts.Files;
using TwentyNet.Domain.Enums;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.BFF.Controllers;

[ApiController]
[Authorize]
[Route("api/files")]
public sealed class FilesController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IMapper _mapper;
    private readonly IStorageDriver _storageDriver;

    public FilesController(ISender sender, IMapper mapper, IStorageDriver storageDriver)
    {
        _sender = sender;
        _mapper = mapper;
        _storageDriver = storageDriver;
    }

    [HttpPost]
    public async Task<ActionResult<FileUploadResponse>> CreateUpload([FromBody] CreateFileUploadRequest request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<FileFolder>(request.Folder, ignoreCase: true, out var folder))
        {
            return BadRequest(new { error = $"Invalid folder '{request.Folder}'." });
        }

        var command = new CreateFileUploadCommand(request.Name, request.MimeType, request.Size, folder);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(_mapper.Map<FileUploadResponse>(result));
    }

    [HttpPut("{id:guid}/content")]
    [RequestSizeLimit(100_000_000)]
    public async Task<IActionResult> UploadContent(Guid id, IFormFile file, CancellationToken cancellationToken)
    {
        var command = new UploadFileContentCommand(id, file.OpenReadStream(), file.ContentType);
        await _sender.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:guid}/complete")]
    public async Task<IActionResult> CompleteUpload(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new CompleteFileUploadCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetFileDownloadUrlQuery(id), cancellationToken);

        if (!string.IsNullOrEmpty(result.Url))
        {
            return Redirect(result.Url);
        }

        var stream = await _storageDriver.ReadAsync(result.StorageKey, cancellationToken);
        return File(stream, "application/octet-stream");
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteFileCommand(id), cancellationToken);
        return NoContent();
    }
}
