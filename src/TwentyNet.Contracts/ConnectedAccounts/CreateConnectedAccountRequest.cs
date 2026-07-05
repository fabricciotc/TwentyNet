namespace TwentyNet.Contracts.ConnectedAccounts;

public sealed record CreateConnectedAccountRequest(
    string Provider,
    string Email,
    string AccessToken,
    string RefreshToken,
    DateTime? ExpiresAt);
