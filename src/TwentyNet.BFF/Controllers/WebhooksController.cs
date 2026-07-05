using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TwentyNet.Application.Webhooks.CreateWebhook;
using TwentyNet.Application.Webhooks.DeleteWebhook;
using TwentyNet.Application.Webhooks.GetWebhookById;
using TwentyNet.Application.Webhooks.ListWebhooks;
using TwentyNet.Application.Webhooks.UpdateWebhook;
using TwentyNet.Contracts.Webhooks;

namespace TwentyNet.BFF.Controllers;

[ApiController]
[Authorize]
[Route("api/webhooks")]
public sealed class WebhooksController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IMapper _mapper;

    public WebhooksController(ISender sender, IMapper mapper)
    {
        _sender = sender;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<WebhookResponse>>> List(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ListWebhooksQuery(), cancellationToken);
        return Ok(_mapper.Map<IReadOnlyList<WebhookResponse>>(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<WebhookResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetWebhookByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(_mapper.Map<WebhookResponse>(result));
    }

    [HttpPost]
    public async Task<ActionResult<WebhookResponse>> Create([FromBody] CreateWebhookRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateWebhookCommand(request.TargetUrl, request.Secret, request.Events);
        var result = await _sender.Send(command, cancellationToken);
        var response = _mapper.Map<WebhookResponse>(result);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<WebhookResponse>> Update(Guid id, [FromBody] UpdateWebhookRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateWebhookCommand(id, request.TargetUrl, request.Secret, request.Events, request.IsActive);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(_mapper.Map<WebhookResponse>(result));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteWebhookCommand(id), cancellationToken);
        return NoContent();
    }
}
