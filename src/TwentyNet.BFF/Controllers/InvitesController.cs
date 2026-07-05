using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TwentyNet.Application.Workspaces.AcceptInvite;
using TwentyNet.Application.Workspaces.RejectInvite;

namespace TwentyNet.BFF.Controllers;

[ApiController]
[Authorize(Policy = "RequireMember")]
[Route("api/invites")]
public sealed class InvitesController : ControllerBase
{
    private readonly ISender _sender;

    public InvitesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("{token:guid}/accept")]
    public async Task<IActionResult> Accept(Guid token, CancellationToken cancellationToken)
    {
        try
        {
            await _sender.Send(new AcceptInviteCommand(token), cancellationToken);
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPost("{token:guid}/reject")]
    public async Task<IActionResult> Reject(Guid token, CancellationToken cancellationToken)
    {
        try
        {
            await _sender.Send(new RejectInviteCommand(token), cancellationToken);
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
}
