using MediatR;

namespace TwentyNet.Application.Auth.LoginUser;

public sealed record LoginUserCommand(
    string Email,
    string Password,
    Guid WorkspaceId) : IRequest<AuthResponse>;
