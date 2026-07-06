using System.Text;
using TwentyNet.BFF.Services;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Enums;

namespace TwentyNet.Application.Tests.Sso;

public sealed class SamlServiceTests
{
    [Fact]
    public void BuildAuthnRequest_ShouldContainEntityIdAndAcsUrl()
    {
        var service = new SamlService();
        var provider = new SsoProvider
        {
            Name = "Okta",
            EntityId = "https://myapp.example.com",
            SingleSignOnUrl = "https://idp.example.com/sso",
            Type = SsoProviderType.Saml
        };

        var xml = service.BuildAuthnRequest(provider, "https://myapp.example.com/acs");

        Assert.Contains("https://myapp.example.com", xml);
        Assert.Contains("https://idp.example.com/sso", xml);
        Assert.Contains("samlp:AuthnRequest", xml);
    }

    [Fact]
    public void ParseSamlResponse_ShouldExtractEmail()
    {
        var service = new SamlService();
        var xml = @"<samlp:Response xmlns:samlp='urn:oasis:names:tc:SAML:2.0:protocol' xmlns:saml='urn:oasis:names:tc:SAML:2.0:assertion'>
            <samlp:Status><samlp:StatusCode Value='urn:oasis:names:tc:SAML:2.0:status:Success'/></samlp:Status>
            <saml:Assertion>
                <saml:Subject><saml:NameID>user@example.com</saml:NameID></saml:Subject>
                <saml:AttributeStatement>
                    <saml:Attribute Name='firstName'><saml:AttributeValue>John</saml:AttributeValue></saml:Attribute>
                    <saml:Attribute Name='lastName'><saml:AttributeValue>Doe</saml:AttributeValue></saml:Attribute>
                </saml:AttributeStatement>
            </saml:Assertion>
        </samlp:Response>";
        var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(xml));

        var result = service.ParseSamlResponse(base64);

        Assert.Equal("user@example.com", result.Email);
        Assert.Equal("John", result.FirstName);
        Assert.Equal("Doe", result.LastName);
    }

    [Fact]
    public void ParseSamlResponse_ShouldThrow_WhenStatusNotSuccess()
    {
        var service = new SamlService();
        var xml = @"<samlp:Response xmlns:samlp='urn:oasis:names:tc:SAML:2.0:protocol'>
            <samlp:Status><samlp:StatusCode Value='urn:oasis:names:tc:SAML:2.0:status:AuthnFailed'/></samlp:Status>
        </samlp:Response>";
        var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(xml));

        Assert.Throws<InvalidOperationException>(() => service.ParseSamlResponse(base64));
    }
}
