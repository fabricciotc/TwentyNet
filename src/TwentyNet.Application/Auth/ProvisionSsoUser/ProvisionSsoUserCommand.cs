using MediatR;
using TwentyNet.Domain.Enums;

namespace TwentyNet.Application.Auth.ProvisionSsoUser;

public sealed record ProvisionSsoUserCommand(
    string Email,
    string? FirstName,
    string? LastName,
    Guid WorkspaceId,
    WorkspaceRole Role) : IRequest<AuthResponse>;
