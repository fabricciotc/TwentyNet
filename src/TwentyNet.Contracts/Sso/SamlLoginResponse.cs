namespace TwentyNet.Contracts.Sso;

public sealed record SamlLoginResponse(
    string SingleSignOnUrl,
    string SamlRequest,
    string RelayState);
