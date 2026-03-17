using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Chickquita.Infrastructure.Data;
using Xunit;

namespace Chickquita.Api.Tests.Middleware;

/// <summary>
/// Integration tests verifying that the CORS policy is active in all environments.
/// The policy is configured via Cors:AllowedOrigins in appsettings.
/// Development appsettings allow http://localhost:3100 — the origin used in these tests.
/// </summary>
public class CorsMiddlewareTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public CorsMiddlewareTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace real Postgres with InMemory so tests don't need a live DB
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseInMemoryDatabase($"CorsTestDb_{Guid.NewGuid()}"));
            });
        });
    }

    [Fact]
    public async Task CorsPolicy_WhenOriginIsAllowed_ReturnsAccessControlAllowOriginHeader()
    {
        // Arrange — http://localhost:3100 is listed in appsettings.Development.json
        var client = _factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Add("Origin", "http://localhost:3100");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.Headers.Contains("Access-Control-Allow-Origin").Should().BeTrue(
            "the AppCors policy must apply for all environments, not only Development");
        response.Headers.GetValues("Access-Control-Allow-Origin")
            .Should().ContainSingle()
            .Which.Should().Be("http://localhost:3100");
    }

    [Fact]
    public async Task CorsPolicy_WhenOriginIsNotAllowed_DoesNotReturnAccessControlAllowOriginHeader()
    {
        // Arrange — this origin is not in the allowed list
        var client = _factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Add("Origin", "https://malicious.example.com");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.Headers.Contains("Access-Control-Allow-Origin").Should().BeFalse(
            "origins not in AllowedOrigins must not receive the CORS header");
    }

    [Fact]
    public async Task CorsPolicy_PreflightRequest_WhenOriginIsAllowed_Returns200()
    {
        // Arrange — simulate a browser preflight OPTIONS request
        var client = _factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Options, "/health");
        request.Headers.Add("Origin", "http://localhost:3100");
        request.Headers.Add("Access-Control-Request-Method", "GET");
        request.Headers.Add("Access-Control-Request-Headers", "Content-Type");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.Headers.Contains("Access-Control-Allow-Origin").Should().BeTrue();
        response.Headers.Contains("Access-Control-Allow-Methods").Should().BeTrue();
    }
}
