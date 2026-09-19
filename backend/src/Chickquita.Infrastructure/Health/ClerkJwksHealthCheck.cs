using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Chickquita.Infrastructure.Health;

/// <summary>
/// Verifies that Clerk's OIDC metadata and JWKS signing keys can actually be resolved.
/// <para>
/// Without this check the API starts and reports itself healthy even when the signing keys
/// were never retrieved, and then rejects every authenticated request with a bare
/// <c>401 Unauthorized</c> ("The signature key was not found") until the process is restarted.
/// </para>
/// </summary>
public sealed class ClerkJwksHealthCheck : IHealthCheck
{
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

            if (signingKeyCount == 0)
            {
                _logger.LogError(
                    "Clerk OIDC metadata for {Authority} resolved but contained no signing key. "
                    + "All authenticated requests will fail with 401.",
                    options.Authority);

                return HealthCheckResult.Unhealthy(
                    $"Clerk OIDC metadata for '{options.Authority}' contained no signing key.");
            }

            return HealthCheckResult.Healthy(
                $"Clerk JWKS resolved with {signingKeyCount} signing key(s).");
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
}
