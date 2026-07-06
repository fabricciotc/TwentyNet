using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TwentyNet.Domain.Enums;
using TwentyNet.Persistence;

namespace TwentyNet.BFF.Auth;

public sealed class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "ApiKey";
    public const string HeaderName = "X-Api-Key";

    private readonly AppDbContext _context;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        AppDbContext context)
        : base(options, logger, encoder)
    {
        _context = context;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(HeaderName, out var headerValue))
        {
            return AuthenticateResult.NoResult();
        }

        var providedKey = headerValue.ToString();
        if (string.IsNullOrWhiteSpace(providedKey))
        {
            return AuthenticateResult.Fail("API key is empty.");
        }

        var prefix = providedKey.Length >= 8 ? providedKey[..8] : providedKey;
        var hash = ComputeHash(providedKey);

        var apiKey = await _context.ApiKeys
            .AsNoTracking()
            .FirstOrDefaultAsync(a =>
                a.KeyPrefix == prefix
                && a.KeyHash == hash
                && a.IsActive
                && (a.ExpiresAt == null || a.ExpiresAt > DateTime.UtcNow));

        if (apiKey is null)
        {
            return AuthenticateResult.Fail("Invalid or expired API key.");
        }

        var claims = new List<Claim>
        {
            new("workspace_id", apiKey.WorkspaceId.ToString()),
            new("role", apiKey.Role.ToString()),
            new(ClaimTypes.Name, $"apikey:{apiKey.Name}")
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }

    private static string ComputeHash(string plainKey)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(plainKey));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
