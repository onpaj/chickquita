using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Chickquita.Infrastructure.Health;

/// <summary>
/// Verifies that Clerk's OIDC metadata and JWKS signing keys can actually be resolved.
/// <para>
/// Without this check the API starts and reports itself healthy even when the signing keys
/// were never retrieved, and then rejects every authenticated request with a bare
/// <c>401 Unauthorized</c> ("The signature key was not found") until the process is restarted.
/// </para>
/// <para>
/// When no signing key is resolved, the description carries enough diagnostics to tell
/// apart the possible causes without shell access to the container: the advertised
/// <c>jwks_uri</c>, the raw keys received, a direct fetch of the JWKS with its CDN cache
/// headers, and an explicit RSA import of the first key.
/// </para>
/// </summary>
public sealed class ClerkJwksHealthCheck : IHealthCheck
{
    private static readonly TimeSpan DiagnosticFetchTimeout = TimeSpan.FromSeconds(10);
    private const int DiagnosticBodyPreviewLength = 300;

    private readonly IOptionsMonitor<JwtBearerOptions> _jwtBearerOptions;
    private readonly ILogger<ClerkJwksHealthCheck> _logger;

    public ClerkJwksHealthCheck(
        IOptionsMonitor<JwtBearerOptions> jwtBearerOptions,
        ILogger<ClerkJwksHealthCheck> logger)
    {
        _jwtBearerOptions = jwtBearerOptions;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var options = _jwtBearerOptions.Get(JwtBearerDefaults.AuthenticationScheme);
        var configurationManager = options.ConfigurationManager;

        if (configurationManager is null)
        {
            _logger.LogError("JWT bearer authentication has no configuration manager; Clerk authority is not configured.");
            return HealthCheckResult.Unhealthy(
                "JWT bearer authentication is not configured with a Clerk authority.");
        }

        try
        {
            var configuration = await configurationManager.GetConfigurationAsync(cancellationToken);
            var signingKeyCount = configuration.SigningKeys.Count;

            if (signingKeyCount > 0)
            {
                return HealthCheckResult.Healthy(
                    $"Clerk JWKS resolved with {signingKeyCount} signing key(s).");
            }

            var diagnostics = await DescribeMissingKeysAsync(configuration, cancellationToken);

            _logger.LogError(
                "Clerk OIDC metadata for {Authority} resolved but contained no signing key. "
                + "All authenticated requests will fail with 401. {Diagnostics}",
                options.Authority,
                diagnostics);

            return HealthCheckResult.Unhealthy(
                $"Clerk OIDC metadata for '{options.Authority}' contained no signing key. {diagnostics}");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to retrieve Clerk OIDC metadata from {Authority}.",
                options.Authority);

            return HealthCheckResult.Unhealthy(
                $"Failed to retrieve Clerk OIDC metadata from '{options.Authority}'.",
                ex);
        }
    }

    private static async Task<string> DescribeMissingKeysAsync(
        OpenIdConnectConfiguration configuration,
        CancellationToken cancellationToken)
    {
        var sb = new StringBuilder();
        var rawKeys = configuration.JsonWebKeySet?.Keys ?? [];

        sb.Append("jwks_uri=").Append(string.IsNullOrEmpty(configuration.JwksUri) ? "<none>" : configuration.JwksUri);
        sb.Append("; rawKeys=").Append(rawKeys.Count);

        foreach (var key in rawKeys)
        {
            sb.Append(" [kid=").Append(key.Kid)
              .Append(" kty=").Append(key.Kty)
              .Append(" use=").Append(key.Use)
              .Append(" alg=").Append(key.Alg)
              .Append(" n=").Append(key.N?.Length ?? 0)
              .Append(" e=").Append(key.E?.Length ?? 0)
              .Append(']');
        }

        var firstRsaKey = rawKeys.FirstOrDefault(k => k.Kty == JsonWebAlgorithmsKeyTypes.RSA);
        if (firstRsaKey is not null)
            sb.Append("; rsaImport=").Append(TryImportRsa(firstRsaKey));

        if (!string.IsNullOrEmpty(configuration.JwksUri))
            sb.Append("; directFetch=").Append(await DescribeDirectFetchAsync(configuration.JwksUri, cancellationToken));

        return sb.ToString();
    }

    /// <summary>
    /// Reproduces the key conversion IdentityModel performs silently (it drops keys it
    /// cannot convert without throwing), surfacing the underlying crypto error if any.
    /// </summary>
    private static string TryImportRsa(JsonWebKey key)
    {
        try
        {
            using var rsa = RSA.Create();
            rsa.ImportParameters(new RSAParameters
            {
                Modulus = Base64UrlEncoder.DecodeBytes(key.N),
                Exponent = Base64UrlEncoder.DecodeBytes(key.E)
            });
            return $"ok({rsa.KeySize}bit)";
        }
        catch (Exception ex)
        {
            return $"FAILED({ex.GetType().Name}: {ex.Message})";
        }
    }

    /// <summary>
    /// Fetches the JWKS directly from inside the container, using a throwaway client since
    /// this only runs on the failure path. Shows exactly what this network identity receives.
    /// </summary>
    private static async Task<string> DescribeDirectFetchAsync(string jwksUri, CancellationToken cancellationToken)
    {
        try
        {
            using var client = new HttpClient { Timeout = DiagnosticFetchTimeout };
            using var response = await client.GetAsync(jwksUri, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            string Header(string name) =>
                response.Headers.TryGetValues(name, out var values) ? string.Join(",", values) : "-";

            var preview = body.Length > DiagnosticBodyPreviewLength
                ? body[..DiagnosticBodyPreviewLength] + "…"
                : body;

            return $"HTTP {(int)response.StatusCode} cf-ray={Header("cf-ray")} "
                 + $"cf-cache-status={Header("cf-cache-status")} age={Header("age")} "
                 + $"len={body.Length} body={preview}";
        }
        catch (Exception ex)
        {
            return $"FAILED({ex.GetType().Name}: {ex.Message})";
        }
    }
}
