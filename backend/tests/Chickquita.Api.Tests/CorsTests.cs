using System.Net;
using Chickquita.Infrastructure.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Chickquita.Api.Tests;

/// <summary>
/// Verifies that CORS is applied in all environments and that startup fails fast
/// when Cors:AllowedOrigins is missing or empty.
/// </summary>
public class CorsTests
{
    private static WebApplicationFactory<Program> CreateFactory(
        Action<IServiceCollection>? configureServices = null)
    {
        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            // Replace real Postgres with InMemory so tests don't need a real DB
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseInMemoryDatabase($"CorsTestDb_{Guid.NewGuid()}"));

                configureServices?.Invoke(services);
            });
        });
    }

    [Fact]
    public async Task UseCors_WhenOriginIsAllowed_ReturnsAccessControlHeader()
    {
        // Arrange — the test environment (Development) has Cors:AllowedOrigins = ["http://localhost:3100"]
        using var factory = CreateFactory();
        var client = factory.CreateClient();

        using var request = new HttpRequestMessage(HttpMethod.Options, "/health");
        request.Headers.Add("Origin", "http://localhost:3100");
        request.Headers.Add("Access-Control-Request-Method", "GET");

        // Act
        var response = await client.SendAsync(request);

        // Assert — CORS preflight should respond (even if the endpoint itself would redirect/auth)
        // The key assertion is that the Access-Control-Allow-Origin header is present
        response.Headers.Contains("Access-Control-Allow-Origin").Should().BeTrue(
            "CORS middleware should be applied for all environments, not just Development");
    }

    [Fact]
    public async Task UseCors_WhenOriginIsNotAllowed_DoesNotReturnAccessControlHeader()
    {
        // Arrange
        using var factory = CreateFactory();
        var client = factory.CreateClient();

        using var request = new HttpRequestMessage(HttpMethod.Options, "/health");
        request.Headers.Add("Origin", "https://attacker.example.com");
        request.Headers.Add("Access-Control-Request-Method", "GET");

        // Act
        var response = await client.SendAsync(request);

        // Assert — an origin not in the allowed list should not receive the CORS header
        response.Headers.Contains("Access-Control-Allow-Origin").Should().BeFalse(
            "CORS should not allow requests from unlisted origins");
    }

    [Fact]
    public void WebApplication_WhenCorsAllowedOriginsMissing_ThrowsInvalidOperationException()
    {
        // Arrange — override config to remove Cors:AllowedOrigins entirely
        var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Cors:AllowedOrigins:0", "");

            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseInMemoryDatabase($"CorsFailFastDb_{Guid.NewGuid()}"));
            });
        });

        // Act — creating the client triggers WebApplication.Build() which runs our validation
        var act = () => factory.CreateClient();

        // Assert
        act.Should().Throw<Exception>()
            .WithMessage("*Cors:AllowedOrigins*");
    }
}
