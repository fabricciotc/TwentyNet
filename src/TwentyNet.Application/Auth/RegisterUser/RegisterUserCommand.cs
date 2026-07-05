using MediatR;

namespace TwentyNet.Application.Auth.RegisterUser;

public sealed record RegisterUserCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string WorkspaceName) : IRequest<AuthResponse>;
