using System.Security.Cryptography;
using System.Text;
using System.Xml;
using TwentyNet.Domain.Entities;

namespace TwentyNet.BFF.Services;

public sealed class SamlService
{
    public string BuildAuthnRequest(SsoProvider provider, string assertionConsumerServiceUrl)
    {
        var requestId = $"_{Guid.NewGuid()}";
        var issueInstant = DateTime.UtcNow.ToString("O");

        var xml = $@"<samlp:AuthnRequest xmlns:samlp=""urn:oasis:names:tc:SAML:2.0:protocol""
            xmlns:saml=""urn:oasis:names:tc:SAML:2.0:assertion""
            ID=""{requestId}""
            Version=""2.0""
            IssueInstant=""{issueInstant}""
            Destination=""{provider.SingleSignOnUrl}""
            AssertionConsumerServiceURL=""{assertionConsumerServiceUrl}"">
            <saml:Issuer>{provider.EntityId}</saml:Issuer>
            <samlp:NameIDPolicy Format=""urn:oasis:names:tc:SAML:1.1:nameid-format:emailAddress"" AllowCreate=""true""/>
        </samlp:AuthnRequest>";

        return xml;
    }

    public SamlResponse ParseSamlResponse(string base64Response)
    {
        var bytes = Convert.FromBase64String(base64Response);
        var xml = Encoding.UTF8.GetString(bytes);
        var doc = new XmlDocument { PreserveWhitespace = true };
        doc.LoadXml(xml);

        var namespaceManager = new XmlNamespaceManager(doc.NameTable);
        namespaceManager.AddNamespace("saml", "urn:oasis:names:tc:SAML:2.0:assertion");
        namespaceManager.AddNamespace("samlp", "urn:oasis:names:tc:SAML:2.0:protocol");

        var statusNode = doc.SelectSingleNode("//samlp:Status/samlp:StatusCode", namespaceManager);
        var status = statusNode?.Attributes?["Value"]?.Value;

        if (status != "urn:oasis:names:tc:SAML:2.0:status:Success")
        {
            throw new InvalidOperationException($"SAML response status is not success: {status}");
        }

        var nameIdNode = doc.SelectSingleNode("//saml:Assertion/saml:Subject/saml:NameID", namespaceManager);
        var email = nameIdNode?.InnerText;

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new InvalidOperationException("SAML response does not contain a NameID email.");
        }

        var firstName = doc.SelectSingleNode("//saml:Attribute[@Name='firstName']/saml:AttributeValue", namespaceManager)?.InnerText;
        var lastName = doc.SelectSingleNode("//saml:Attribute[@Name='lastName']/saml:AttributeValue", namespaceManager)?.InnerText;

        return new SamlResponse(email, firstName, lastName);
    }

    public static string ComputeHash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}

public sealed record SamlResponse(string Email, string? FirstName, string? LastName);
