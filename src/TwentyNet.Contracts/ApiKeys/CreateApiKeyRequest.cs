namespace TwentyNet.Contracts.ApiKeys;

public sealed record CreateApiKeyRequest(
    string Name,
    string Role,
    IReadOnlyList<string>? Scopes = null,
    DateTime? ExpiresAt = null);
