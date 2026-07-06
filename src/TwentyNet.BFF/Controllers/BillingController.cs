using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TwentyNet.Application.Billing.CancelSubscription;
using TwentyNet.Application.Billing.GenerateInvoice;
using TwentyNet.Application.Billing.GetSubscription;
using TwentyNet.Application.Billing.ListInvoices;
using TwentyNet.Application.Billing.ListPlans;
using TwentyNet.Application.Billing.SubscribeWorkspace;
using TwentyNet.Contracts.Billing;

namespace TwentyNet.BFF.Controllers;

[ApiController]
[Authorize(Policy = "RequireAdmin")]
[Route("api/billing")]
public sealed class BillingController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IMapper _mapper;

    public BillingController(ISender sender, IMapper mapper)
    {
        _sender = sender;
        _mapper = mapper;
    }

    [HttpGet("plans")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<SubscriptionPlanResponse>>> ListPlans(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ListSubscriptionPlansQuery(), cancellationToken);
        return Ok(_mapper.Map<IReadOnlyList<SubscriptionPlanResponse>>(result));
    }

    [HttpGet("subscription")]
    public async Task<ActionResult<WorkspaceSubscriptionResponse>> GetSubscription(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetWorkspaceSubscriptionQuery(), cancellationToken);
        return result is null ? NotFound() : Ok(_mapper.Map<WorkspaceSubscriptionResponse>(result));
    }

    [HttpPost("subscribe")]
    public async Task<ActionResult<WorkspaceSubscriptionResponse>> Subscribe(
        [FromBody] SubscribeWorkspaceRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new SubscribeWorkspaceCommand(request.PlanId), cancellationToken);
        return Ok(_mapper.Map<WorkspaceSubscriptionResponse>(result));
    }

    [HttpPost("subscription/{subscriptionId:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid subscriptionId, CancellationToken cancellationToken)
    {
        await _sender.Send(new CancelSubscriptionCommand(subscriptionId), cancellationToken);
        return NoContent();
    }

    [HttpGet("invoices")]
    public async Task<ActionResult<IReadOnlyList<InvoiceResponse>>> ListInvoices(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ListInvoicesQuery(), cancellationToken);
        return Ok(_mapper.Map<IReadOnlyList<InvoiceResponse>>(result));
    }

    [HttpPost("invoices")]
    public async Task<ActionResult<InvoiceResponse>> GenerateInvoice(
        [FromBody] GenerateInvoiceRequest request,
        CancellationToken cancellationToken)
    {
        // In a real app this would be driven by the billing provider webhooks.
        // Exposed here for admin/testing convenience.
        var workspaceId = HttpContext.User.Claims
            .FirstOrDefault(c => c.Type == "workspace_id")?.Value;

        if (!Guid.TryParse(workspaceId, out var parsedWorkspaceId))
        {
            return Unauthorized();
        }

        var result = await _sender.Send(
            new GenerateInvoiceCommand(parsedWorkspaceId, request.Amount, request.Currency, request.PeriodStart, request.PeriodEnd),
            cancellationToken);

        return Ok(_mapper.Map<InvoiceResponse>(result));
    }
}
