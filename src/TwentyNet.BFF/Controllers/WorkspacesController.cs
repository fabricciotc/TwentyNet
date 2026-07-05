using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TwentyNet.Application.Workspaces.GetWorkspaceMembers;
using TwentyNet.Application.Workspaces.InviteMember;
using TwentyNet.Application.Workspaces.ListUserWorkspaces;
using TwentyNet.Application.Workspaces.RemoveMember;
using TwentyNet.Application.Workspaces.UpdateMemberRole;
using TwentyNet.Domain.Enums;
using TwentyNet.Contracts.Workspaces;

namespace TwentyNet.BFF.Controllers;

[ApiController]
[Authorize(Policy = "RequireMember")]
[Route("api/workspaces")]
public sealed class WorkspacesController : ControllerBase
{
    private readonly ISender _sender;

    public WorkspacesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<WorkspaceResponse>>> List(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ListUserWorkspacesQuery(), cancellationToken);
        return Ok(result.Select(MapToResponse));
    }

    [HttpGet("{id:guid}/members")]
    public async Task<ActionResult<IReadOnlyList<WorkspaceMemberResponse>>> GetMembers(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetWorkspaceMembersQuery(id), cancellationToken);
        return Ok(result.Select(MapToMemberResponse));
    }

    [HttpPost("{id:guid}/invites")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<ActionResult<WorkspaceInviteResponse>> InviteMember(
        Guid id,
        [FromBody] InviteMemberRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<WorkspaceRole>(request.Role, true, out var role))
        {
            return BadRequest("Invalid role.");
        }

        var command = new InviteMemberCommand(id, request.Email, role);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(MapToInviteResponse(result));
    }

    [HttpPut("{id:guid}/members/{userId:guid}/role")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> UpdateMemberRole(
        Guid id,
        Guid userId,
        [FromBody] UpdateMemberRoleRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<WorkspaceRole>(request.Role, true, out var role))
        {
            return BadRequest("Invalid role.");
        }

        await _sender.Send(new UpdateMemberRoleCommand(id, userId, role), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}/members/{userId:guid}")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> RemoveMember(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        await _sender.Send(new RemoveMemberCommand(id, userId), cancellationToken);
        return NoContent();
    }

    private static WorkspaceResponse MapToResponse(Application.Workspaces.WorkspaceDto dto)
    {
        return new WorkspaceResponse(dto.Id, dto.Name, dto.Role.ToString(), dto.CreatedAt);
    }

    private static WorkspaceMemberResponse MapToMemberResponse(Application.Workspaces.WorkspaceMemberDto dto)
    {
        return new WorkspaceMemberResponse(dto.UserId, dto.Email, dto.FirstName, dto.LastName, dto.Role.ToString());
    }

    private static WorkspaceInviteResponse MapToInviteResponse(Application.Workspaces.WorkspaceInviteDto dto)
    {
        return new WorkspaceInviteResponse(
            dto.Id,
            dto.WorkspaceId,
            dto.Email,
            dto.Role.ToString(),
            dto.Token,
            dto.ExpiresAt,
            dto.AcceptedAt,
            dto.RejectedAt);
    }
}
