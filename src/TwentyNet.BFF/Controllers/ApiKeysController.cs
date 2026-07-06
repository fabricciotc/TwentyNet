using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TwentyNet.Application.ApiKeys.DeleteApiKey;
using TwentyNet.Application.ApiKeys.GenerateApiKey;
using TwentyNet.Application.ApiKeys.GetApiKeyById;
using TwentyNet.Application.ApiKeys.ListApiKeys;
using TwentyNet.Application.ApiKeys.RevokeApiKey;
using TwentyNet.Contracts.ApiKeys;
using DomainEnums = TwentyNet.Domain.Enums;

namespace TwentyNet.BFF.Controllers;

[ApiController]
[Authorize(Policy = "RequireAdmin")]
[Route("api/api-keys")]
public sealed class ApiKeysController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IMapper _mapper;

    public ApiKeysController(ISender sender, IMapper mapper)
    {
        _sender = sender;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ApiKeyResponse>>> List(
        [FromQuery] bool? isActive,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ListApiKeysQuery(isActive), cancellationToken);
        return Ok(_mapper.Map<IReadOnlyList<ApiKeyResponse>>(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiKeyResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetApiKeyByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(_mapper.Map<ApiKeyResponse>(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiKeyCreatedResponse>> Create(
        [FromBody] CreateApiKeyRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<DomainEnums.WorkspaceRole>(request.Role, true, out var role))
        {
            return BadRequest($"Invalid role '{request.Role}'.");
        }

        var command = new GenerateApiKeyCommand(request.Name, role, request.Scopes, request.ExpiresAt);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(_mapper.Map<ApiKeyCreatedResponse>(result));
    }

    [HttpPut("{id:guid}/revoke")]
    public async Task<IActionResult> Revoke(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new RevokeApiKeyCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteApiKeyCommand(id), cancellationToken);
        return NoContent();
    }
}
