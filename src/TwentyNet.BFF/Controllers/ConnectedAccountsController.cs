using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TwentyNet.Application.ConnectedAccounts.CreateConnectedAccount;
using TwentyNet.Application.ConnectedAccounts.DeleteConnectedAccount;
using TwentyNet.Application.ConnectedAccounts.GetConnectedAccountById;
using TwentyNet.Application.ConnectedAccounts.ListConnectedAccounts;
using TwentyNet.Contracts.ConnectedAccounts;
using TwentyNet.Domain.Enums;

namespace TwentyNet.BFF.Controllers;

[ApiController]
[Authorize]
[Route("api/connected-accounts")]
public sealed class ConnectedAccountsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IMapper _mapper;

    public ConnectedAccountsController(ISender sender, IMapper mapper)
    {
        _sender = sender;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ConnectedAccountResponse>>> List(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ListConnectedAccountsQuery(), cancellationToken);
        return Ok(_mapper.Map<IReadOnlyList<ConnectedAccountResponse>>(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ConnectedAccountResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetConnectedAccountByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(_mapper.Map<ConnectedAccountResponse>(result));
    }

    [HttpPost]
    public async Task<ActionResult<ConnectedAccountResponse>> Create([FromBody] CreateConnectedAccountRequest request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<ConnectorProvider>(request.Provider, true, out var provider))
        {
            return BadRequest($"Invalid provider '{request.Provider}'.");
        }

        var command = new CreateConnectedAccountCommand(
            provider,
            request.Email,
            request.AccessToken,
            request.RefreshToken,
            request.ExpiresAt);

        var result = await _sender.Send(command, cancellationToken);
        var response = _mapper.Map<ConnectedAccountResponse>(result);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteConnectedAccountCommand(id), cancellationToken);
        return NoContent();
    }
}
