using System.Net;
using System.Text.Json;
using Chickquita.Infrastructure.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Chickquita.Api.Tests.Endpoints;

public class HealthCheckEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public HealthCheckEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace real Postgres with InMemory so the DB health check works in tests
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseInMemoryDatabase($"HealthCheckTestDb_{Guid.NewGuid()}"));
            });
        });
    }

    [Fact]
    public async Task GetHealth_ReturnsOkWithJsonBody()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        doc.RootElement.TryGetProperty("status", out var status).Should().BeTrue();
        status.GetString().Should().Be("Healthy");
        doc.RootElement.TryGetProperty("entries", out _).Should().BeTrue();
        doc.RootElement.TryGetProperty("totalDuration", out _).Should().BeTrue();
    }

    [Fact]
    public async Task GetHealthLive_ReturnsOkWithJsonBody()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health/live");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        doc.RootElement.TryGetProperty("status", out var status).Should().BeTrue();
        status.GetString().Should().Be("Healthy");
        // Liveness has no checks — entries should be empty
        doc.RootElement.TryGetProperty("entries", out var entries).Should().BeTrue();
        entries.EnumerateObject().Should().BeEmpty();
    }

    [Fact]
    public async Task GetHealthReady_ReturnsOkWhenDatabaseHealthy()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health/ready");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        doc.RootElement.TryGetProperty("status", out var status).Should().BeTrue();
        status.GetString().Should().Be("Healthy");
        doc.RootElement.TryGetProperty("entries", out var entries).Should().BeTrue();
        entries.TryGetProperty("database", out var dbEntry).Should().BeTrue();
        dbEntry.GetProperty("status").GetString().Should().Be("Healthy");
    }

    [Fact]
    public async Task HealthEndpoints_DoNotRequireAuthentication()
    {
        var client = _factory.CreateClient();

        // Hit all three without any Authorization header
        var healthResponse = await client.GetAsync("/health");
        var liveResponse = await client.GetAsync("/health/live");
        var readyResponse = await client.GetAsync("/health/ready");

        healthResponse.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        liveResponse.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        readyResponse.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }
}
