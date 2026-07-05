using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace TwentyNet.BFF.Hubs;

[Authorize]
public sealed class WorkspaceHub : Hub<IWorkspaceClient>
{
    private const string WorkspaceIdClaimType = "workspace_id";

    public override async Task OnConnectedAsync()
    {
        var workspaceId = Context.User?.FindFirst(WorkspaceIdClaimType)?.Value;

        if (!string.IsNullOrWhiteSpace(workspaceId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"workspace:{workspaceId}");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var workspaceId = Context.User?.FindFirst(WorkspaceIdClaimType)?.Value;

        if (!string.IsNullOrWhiteSpace(workspaceId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"workspace:{workspaceId}");
        }

        await base.OnDisconnectedAsync(exception);
    }
}
