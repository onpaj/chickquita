using Chickquita.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
}
