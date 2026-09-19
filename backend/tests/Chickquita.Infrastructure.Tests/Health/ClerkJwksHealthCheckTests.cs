using System.Security.Cryptography;
using Chickquita.Infrastructure.Health;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Chickquita.Infrastructure.Tests.Health;

/// <summary>
/// Tests that the Clerk JWKS health check surfaces signing-key resolution failures,
/// which otherwise let the app start "healthy" while rejecting every authenticated request.
/// <para>
/// Failures report <see cref="HealthStatus.Degraded"/>, not Unhealthy: the app still serves
/// the SPA and public endpoints, and /health must keep returning 200 so a Clerk outage cannot
/// block the CI readiness gate and stall a deployment.
/// </para>
/// </summary>
public class ClerkJwksHealthCheckTests
{
    private const string Authority = "https://clerk.example.com";

    [Fact]
    public async Task CheckHealthAsync_WhenSigningKeysPresent_ReturnsHealthy()
    {
        // Arrange
        var config = new OpenIdConnectConfiguration { Issuer = Authority };
        config.SigningKeys.Add(CreateSigningKey());
        var sut = CreateSut(new StubConfigurationManager(config));

        // Act
        var result = await sut.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
    }

    [Fact]
    public async Task CheckHealthAsync_WhenNoSigningKeysReturned_ReturnsUnhealthy()
    {
        // Arrange — configuration resolves but carries no signing keys
        var config = new OpenIdConnectConfiguration { Issuer = Authority };
        var sut = CreateSut(new StubConfigurationManager(config));

        // Act
        var result = await sut.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

        // Assert
        result.Status.Should().Be(HealthStatus.Degraded);
        result.Description.Should().Contain("signing key");
    }

    [Fact]
    public async Task CheckHealthAsync_WhenMetadataRetrievalThrows_ReturnsUnhealthy()
    {
        // Arrange
        var sut = CreateSut(new StubConfigurationManager(config: null, new HttpRequestException("connection refused")));

        // Act
        var result = await sut.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

        // Assert
        result.Status.Should().Be(HealthStatus.Degraded);
        result.Exception.Should().BeOfType<HttpRequestException>();
    }

    [Fact]
    public async Task CheckHealthAsync_WhenConfigurationManagerMissing_ReturnsUnhealthy()
    {
        // Arrange
        var sut = CreateSut(configurationManager: null);

        // Act
        var result = await sut.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

        // Assert
        result.Status.Should().Be(HealthStatus.Degraded);
    }

    private static ClerkJwksHealthCheck CreateSut(IConfigurationManager<OpenIdConnectConfiguration>? configurationManager)
    {
        var options = new JwtBearerOptions
        {
            Authority = Authority,
            ConfigurationManager = configurationManager
        };

        return new ClerkJwksHealthCheck(
            new StubOptionsMonitor(options),
            NullLogger<ClerkJwksHealthCheck>.Instance);
    }

    private static SecurityKey CreateSigningKey()
    {
        var rsa = RSA.Create(2048);
        return new RsaSecurityKey(rsa.ExportParameters(includePrivateParameters: false)) { KeyId = "test-kid" };
    }

    private sealed class StubConfigurationManager(OpenIdConnectConfiguration? config, Exception? failure = null)
        : IConfigurationManager<OpenIdConnectConfiguration>
    {
        public Task<OpenIdConnectConfiguration> GetConfigurationAsync(CancellationToken cancel)
            => failure is not null
                ? Task.FromException<OpenIdConnectConfiguration>(failure)
                : Task.FromResult(config!);

        public void RequestRefresh() { }
    }

    private sealed class StubOptionsMonitor(JwtBearerOptions options) : IOptionsMonitor<JwtBearerOptions>
    {
        public JwtBearerOptions CurrentValue => options;

        public JwtBearerOptions Get(string? name) => options;

        public IDisposable? OnChange(Action<JwtBearerOptions, string?> listener) => null;
    }
}
