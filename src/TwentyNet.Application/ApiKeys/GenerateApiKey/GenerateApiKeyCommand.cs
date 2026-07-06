using MediatR;
using TwentyNet.Domain.Enums;

namespace TwentyNet.Application.ApiKeys.GenerateApiKey;

public sealed record GenerateApiKeyCommand(
    string Name,
    WorkspaceRole Role,
    IReadOnlyList<string>? Scopes = null,
    DateTime? ExpiresAt = null) : IRequest<ApiKeyCreatedDto>;
