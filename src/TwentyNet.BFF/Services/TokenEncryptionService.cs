using Microsoft.AspNetCore.DataProtection;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.BFF.Services;

public sealed class TokenEncryptionService : ITokenEncryptionService
{
    private const string Purpose = "ConnectedAccountTokens";
    private readonly IDataProtector _protector;

    public TokenEncryptionService(IDataProtectionProvider dataProtectionProvider)
    {
        _protector = dataProtectionProvider.CreateProtector(Purpose);
    }

    public string Encrypt(string plainText)
    {
        ArgumentException.ThrowIfNullOrEmpty(plainText);
        return _protector.Protect(plainText);
    }

    public string Decrypt(string cipherText)
    {
        ArgumentException.ThrowIfNullOrEmpty(cipherText);
        return _protector.Unprotect(cipherText);
    }
}
