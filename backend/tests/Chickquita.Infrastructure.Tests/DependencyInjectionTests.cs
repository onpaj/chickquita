using Chickquita.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Chickquita.Infrastructure.Tests;

/// <summary>
/// Tests that AddInfrastructureServices fails fast when required configuration is absent.
/// </summary>
public class DependencyInjectionTests
{
    [Fact]
    public void AddInfrastructureServices_WhenClerkAuthorityMissing_ThrowsInvalidOperationException()
    {
        // Arrange — config without Clerk:Authority
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=test",
                ["ASPNETCORE_ENVIRONMENT"] = "Development"
                // Clerk:Authority intentionally omitted
            })
            .Build();

        var services = new ServiceCollection();

        // Act
        var act = () => services.AddInfrastructureServices(config);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Clerk:Authority*");
    }

    [Fact]
    public void AddInfrastructureServices_WhenClerkAuthorityPresent_DoesNotThrow()
    {
        // Arrange — config with Clerk:Authority set
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=test",
                ["ASPNETCORE_ENVIRONMENT"] = "Development",
                ["Clerk:Authority"] = "https://example.clerk.accounts.dev"
            })
            .Build();

        var services = new ServiceCollection();

        // Act & Assert
        var act = () => services.AddInfrastructureServices(config);
        act.Should().NotThrow();
    }

    [Fact]
    public void AddInfrastructureServices_ConfiguresIssuerValidationIndependentOfDiscovery()
    {
        // Arrange — issuer validation must not depend on the OIDC discovery document,
        // so a JWKS fetch failure cannot silently disable all authentication.
        const string authority = "https://clerk.example.com";
        var provider = BuildProvider(authority);

        // Act
        var options = provider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);

        // Assert
        options.TokenValidationParameters.ValidIssuer.Should().Be(authority);
        options.TokenValidationParameters.ValidateIssuerSigningKey.Should().BeTrue();
    }

    [Fact]
    public void AddInfrastructureServices_EnablesKeyRefreshOnIssuerKeyNotFound()
    {
        // Arrange — a stale/empty key set must trigger a metadata refresh rather than
        // rejecting every request until the process is restarted.
        var provider = BuildProvider("https://clerk.example.com");

        // Act
        var options = provider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);

        // Assert
        options.RefreshOnIssuerKeyNotFound.Should().BeTrue();
    }

    [Fact]
    public void AddInfrastructureServices_LogsAuthenticationFailures()
    {
        // Arrange — auth failures must be logged under our own category, so they are not
        // hidden by the Microsoft.AspNetCore log level being pinned to Warning.
        var provider = BuildProvider("https://clerk.example.com");

        // Act
        var options = provider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);

        // Assert
        options.Events.Should().NotBeNull();
        options.Events!.OnAuthenticationFailed.Should().NotBeNull();
    }

    private static ServiceProvider BuildProvider(string authority)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=test",
                ["ASPNETCORE_ENVIRONMENT"] = "Development",
                ["Clerk:Authority"] = authority
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddInfrastructureServices(config);
        return services.BuildServiceProvider();
    }
}
