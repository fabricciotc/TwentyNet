using MediatR;

namespace TwentyNet.Application.Auth.SwitchWorkspace;

public sealed record SwitchWorkspaceCommand(Guid WorkspaceId) : IRequest<AuthResponse>;
