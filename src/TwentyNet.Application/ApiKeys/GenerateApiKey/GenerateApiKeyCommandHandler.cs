using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.ApiKeys.GenerateApiKey;

public sealed class GenerateApiKeyCommandHandler : IRequestHandler<GenerateApiKeyCommand, ApiKeyCreatedDto>
{
    private readonly IRepository<ApiKey> _repository;
    private readonly IAuthContext _authContext;

    public GenerateApiKeyCommandHandler(IRepository<ApiKey> repository, IAuthContext authContext)
    {
        _repository = repository;
        _authContext = authContext;
    }

    public async Task<ApiKeyCreatedDto> Handle(GenerateApiKeyCommand request, CancellationToken cancellationToken)
    {
        if (!_authContext.WorkspaceId.HasValue)
        {
            throw new UnauthorizedAccessException("Workspace is required.");
        }

        var workspaceId = _authContext.WorkspaceId.Value;
        var plainKey = $"twenty_{Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant()}";
        var prefix = plainKey[..Math.Min(8, plainKey.Length)];
        var hash = ComputeHash(plainKey);
        var expiresAt = request.ExpiresAt ?? DateTime.UtcNow.AddYears(1);

        var apiKey = new ApiKey
        {
            Name = request.Name,
            KeyHash = hash,
            KeyPrefix = prefix,
            WorkspaceId = workspaceId,
            Role = request.Role,
            Scopes = JsonSerializer.Serialize(request.Scopes ?? new List<string>()),
            ExpiresAt = expiresAt,
            IsActive = true
        };

        await _repository.AddAsync(apiKey, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return new ApiKeyCreatedDto(apiKey.Id, apiKey.Name, plainKey, workspaceId, expiresAt);
    }

    public static string ComputeHash(string plainKey)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(plainKey));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
