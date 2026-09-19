using FluentAssertions;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Chickquita.Infrastructure.Tests.Auth;

/// <summary>
/// Guards against mismatched Microsoft.IdentityModel package versions.
/// <para>
/// Pairing Protocols.OpenIdConnect 7.x with Tokens/JsonWebTokens 8.x makes the OIDC metadata
/// serializer silently stop yielding values partway through the document: <c>issuer</c> is
/// parsed but <c>jwks_uri</c> is dropped. No exception is raised, so the API starts normally
/// and then rejects every authenticated request with
/// "The signature key was not found", because no JWKS fetch is ever attempted.
/// </para>
/// </summary>
public class OpenIdConnectMetadataParsingTests
{
    /// Real Clerk discovery document (trimmed), field order preserved.
    private const string ClerkDiscoveryDocument = @"{""issuer"": ""https://clerk.chickquita.com"", ""authorization_endpoint"": ""https://clerk.chickquita.com/oauth/authorize"", ""token_endpoint"": ""https://clerk.chickquita.com/oauth/token"", ""userinfo_endpoint"": ""https://clerk.chickquita.com/oauth/userinfo"", ""device_authorization_endpoint"": ""https://clerk.chickquita.com/oauth/device_authorization"", ""jwks_uri"": ""https://clerk.chickquita.com/.well-known/jwks.json"", ""scopes_supported"": [""profile"", ""public_metadata"", ""private_metadata"", ""openid"", ""offline_access"", ""user:org:read"", ""email""], ""response_types_supported"": [""code""], ""subject_types_supported"": [""public""], ""id_token_signing_alg_values_supported"": [""RS256""], ""claims_supported"": [""exp"", ""iat"", ""email"", ""family_name"", ""picture"", ""iss"", ""email_verified"", ""given_name"", ""name"", ""preferred_username"", ""sub"", ""aud"", ""org_id""]}";

    [Fact]
    public void Create_ParsesJwksUriFromClerkDiscoveryDocument()
    {
        // Act
        var configuration = OpenIdConnectConfiguration.Create(ClerkDiscoveryDocument);

        // Assert — jwks_uri must survive parsing, otherwise no signing key is ever fetched
        configuration.Issuer.Should().Be("https://clerk.chickquita.com");
        configuration.JwksUri.Should().Be("https://clerk.chickquita.com/.well-known/jwks.json");
    }
}
