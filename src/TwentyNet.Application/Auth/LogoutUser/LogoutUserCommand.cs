using MediatR;

namespace TwentyNet.Application.Auth.LogoutUser;

public sealed record LogoutUserCommand(string RefreshToken) : IRequest<Unit>;
