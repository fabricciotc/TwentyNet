using MediatR;

namespace TwentyNet.Application.Auth.RotateToken;

public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResponse>;
