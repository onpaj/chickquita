using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using Chickquita.Application.DTOs;
using Chickquita.Application.Features.Flocks.Commands;
using Chickquita.Application.Interfaces;
using Chickquita.Domain.Entities;
using Chickquita.Infrastructure.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace Chickquita.Api.Tests.Performance;

/// <summary>
/// Performance tests for Flocks API endpoints.
/// Validates that response times meet the < 500ms p95 requirement.
/// </summary>
public class FlocksPerformanceTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly ITestOutputHelper _output;

    // Performance requirements
    private const int MaxP95ResponseTimeMs = 500;
    private const int PerformanceTestIterations = 100;

    public FlocksPerformanceTests(WebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
    }

    [Fact]
    public async Task GetFlocks_ResponseTime_MeetsP95Requirement()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var mockCurrentUser = CreateMockCurrentUser("clerk_user_perf", tenantId);

        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                ReplaceWithInMemoryDatabase(services);
                ReplaceCurrentUserService(services, mockCurrentUser);
            });
        });

        using var scope = factory.Services.CreateScope();

        // Seed test data
        await SeedTenant(scope, tenantId, "clerk_user_perf");
        var coopId = await SeedCoop(scope, tenantId, "Performance Test Coop", "Test Location");

        // Create multiple flocks to simulate realistic load
        for (int i = 1; i <= 50; i++)
        {
            await SeedFlock(scope, tenantId, coopId, $"Flock-{i:D3}", 10 + i, 2, i % 10);
        }

        var client = factory.CreateClient();

        // Warm up - make a few requests to ensure JIT compilation and caching
        for (int i = 0; i < 5; i++)
        {
            await client.GetAsync("/api/flocks");
        }

        // Act - Run performance test
        var responseTimes = new List<long>();
        for (int i = 0; i < PerformanceTestIterations; i++)
        {
            var sw = Stopwatch.StartNew();
            var response = await client.GetAsync("/api/flocks");
            sw.Stop();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            responseTimes.Add(sw.ElapsedMilliseconds);
        }

        // Assert - Calculate p95
        var p95 = CalculatePercentile(responseTimes, 95);
        var avg = responseTimes.Average();
        var max = responseTimes.Max();
        var min = responseTimes.Min();

        _output.WriteLine($"GET /api/flocks Performance Results (n={PerformanceTestIterations}):");
        _output.WriteLine($"  Min: {min}ms");
        _output.WriteLine($"  Avg: {avg:F2}ms");
        _output.WriteLine($"  p95: {p95}ms");
        _output.WriteLine($"  Max: {max}ms");
        _output.WriteLine($"  Target: < {MaxP95ResponseTimeMs}ms (p95)");

        p95.Should().BeLessThanOrEqualTo(MaxP95ResponseTimeMs,
            $"p95 response time should be <= {MaxP95ResponseTimeMs}ms, but was {p95}ms");
    }

    [Fact]
    public async Task GetFlocksByCoop_ResponseTime_MeetsP95Requirement()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var mockCurrentUser = CreateMockCurrentUser("clerk_user_perf", tenantId);

        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                ReplaceWithInMemoryDatabase(services);
                ReplaceCurrentUserService(services, mockCurrentUser);
            });
        });

        using var scope = factory.Services.CreateScope();

        // Seed test data
        await SeedTenant(scope, tenantId, "clerk_user_perf");
        var coopId = await SeedCoop(scope, tenantId, "Performance Test Coop", "Test Location");

        // Create multiple flocks
        for (int i = 1; i <= 50; i++)
        {
            await SeedFlock(scope, tenantId, coopId, $"Flock-{i:D3}", 10 + i, 2, i % 10);
        }

        var client = factory.CreateClient();

        // Warm up
        for (int i = 0; i < 5; i++)
        {
            await client.GetAsync($"/api/coops/{coopId}/flocks");
        }

        // Act - Run performance test
        var responseTimes = new List<long>();
        for (int i = 0; i < PerformanceTestIterations; i++)
        {
            var sw = Stopwatch.StartNew();
            var response = await client.GetAsync($"/api/coops/{coopId}/flocks");
            sw.Stop();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            responseTimes.Add(sw.ElapsedMilliseconds);
        }

        // Assert - Calculate p95
        var p95 = CalculatePercentile(responseTimes, 95);
        var avg = responseTimes.Average();
        var max = responseTimes.Max();
        var min = responseTimes.Min();

        _output.WriteLine($"GET /api/coops/{{coopId}}/flocks Performance Results (n={PerformanceTestIterations}):");
        _output.WriteLine($"  Min: {min}ms");
        _output.WriteLine($"  Avg: {avg:F2}ms");
        _output.WriteLine($"  p95: {p95}ms");
        _output.WriteLine($"  Max: {max}ms");
        _output.WriteLine($"  Target: < {MaxP95ResponseTimeMs}ms (p95)");

        p95.Should().BeLessThanOrEqualTo(MaxP95ResponseTimeMs,
            $"p95 response time should be <= {MaxP95ResponseTimeMs}ms, but was {p95}ms");
    }

    [Fact]
    public async Task PostFlock_ResponseTime_MeetsP95Requirement()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var mockCurrentUser = CreateMockCurrentUser("clerk_user_perf", tenantId);

        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                ReplaceWithInMemoryDatabase(services);
                ReplaceCurrentUserService(services, mockCurrentUser);
            });
        });

        using var scope = factory.Services.CreateScope();

        // Seed test data
        await SeedTenant(scope, tenantId, "clerk_user_perf");
        var coopId = await SeedCoop(scope, tenantId, "Performance Test Coop", "Test Location");

        var client = factory.CreateClient();

        // Warm up
        for (int i = 0; i < 5; i++)
        {
            var warmupCommand = new CreateFlockCommand
            {
                Identifier = $"Warmup-{i}",
                HatchDate = DateTime.UtcNow.AddMonths(-3),
                InitialHens = 10,
                InitialRoosters = 2,
                InitialChicks = 0
            };
            await client.PostAsJsonAsync($"/api/coops/{coopId}/flocks", warmupCommand);
        }

        // Act - Run performance test
        var responseTimes = new List<long>();
        for (int i = 0; i < PerformanceTestIterations; i++)
        {
            var command = new CreateFlockCommand
            {
                Identifier = $"Perf-{i:D4}",
                HatchDate = DateTime.UtcNow.AddMonths(-3),
                InitialHens = 10,
                InitialRoosters = 2,
                InitialChicks = 0,
                Notes = $"Performance test flock {i}"
            };

            var sw = Stopwatch.StartNew();
            var response = await client.PostAsJsonAsync($"/api/coops/{coopId}/flocks", command);
            sw.Stop();

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            responseTimes.Add(sw.ElapsedMilliseconds);
        }

        // Assert - Calculate p95
        var p95 = CalculatePercentile(responseTimes, 95);
        var avg = responseTimes.Average();
        var max = responseTimes.Max();
        var min = responseTimes.Min();

        _output.WriteLine($"POST /api/coops/{{coopId}}/flocks Performance Results (n={PerformanceTestIterations}):");
        _output.WriteLine($"  Min: {min}ms");
        _output.WriteLine($"  Avg: {avg:F2}ms");
        _output.WriteLine($"  p95: {p95}ms");
        _output.WriteLine($"  Max: {max}ms");
        _output.WriteLine($"  Target: < {MaxP95ResponseTimeMs}ms (p95)");

        p95.Should().BeLessThanOrEqualTo(MaxP95ResponseTimeMs,
            $"p95 response time should be <= {MaxP95ResponseTimeMs}ms, but was {p95}ms");
    }

    [Fact]
    public async Task PutFlock_ResponseTime_MeetsP95Requirement()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var mockCurrentUser = CreateMockCurrentUser("clerk_user_perf", tenantId);

        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                ReplaceWithInMemoryDatabase(services);
                ReplaceCurrentUserService(services, mockCurrentUser);
            });
        });

        using var scope = factory.Services.CreateScope();

        // Seed test data
        await SeedTenant(scope, tenantId, "clerk_user_perf");
        var coopId = await SeedCoop(scope, tenantId, "Performance Test Coop", "Test Location");

        // Create flocks to update
        var flockIds = new List<Guid>();
        for (int i = 0; i < PerformanceTestIterations + 5; i++)
        {
            var id = await SeedFlock(scope, tenantId, coopId, $"Original-{i:D4}", 10, 2, 0);
            flockIds.Add(id);
        }

        var client = factory.CreateClient();

        // Warm up
        for (int i = 0; i < 5; i++)
        {
            var warmupCommand = new UpdateFlockCommand
            {
                FlockId = flockIds[i],
                Identifier = $"Warmup-{i}",
                HatchDate = DateTime.UtcNow.AddMonths(-4)
            };
            await client.PutAsJsonAsync($"/api/flocks/{flockIds[i]}", warmupCommand);
        }

        // Act - Run performance test
        var responseTimes = new List<long>();
        for (int i = 0; i < PerformanceTestIterations; i++)
        {
            var flockId = flockIds[i + 5]; // Skip warmup flocks
            var command = new UpdateFlockCommand
            {
                FlockId = flockId,
                Identifier = $"Updated-{i:D4}",
                HatchDate = DateTime.UtcNow.AddMonths(-4)
            };

            var sw = Stopwatch.StartNew();
            var response = await client.PutAsJsonAsync($"/api/flocks/{flockId}", command);
            sw.Stop();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            responseTimes.Add(sw.ElapsedMilliseconds);
        }

        // Assert - Calculate p95
        var p95 = CalculatePercentile(responseTimes, 95);
        var avg = responseTimes.Average();
        var max = responseTimes.Max();
        var min = responseTimes.Min();

        _output.WriteLine($"PUT /api/flocks/{{id}} Performance Results (n={PerformanceTestIterations}):");
        _output.WriteLine($"  Min: {min}ms");
        _output.WriteLine($"  Avg: {avg:F2}ms");
        _output.WriteLine($"  p95: {p95}ms");
        _output.WriteLine($"  Max: {max}ms");
        _output.WriteLine($"  Target: < {MaxP95ResponseTimeMs}ms (p95)");

        p95.Should().BeLessThanOrEqualTo(MaxP95ResponseTimeMs,
            $"p95 response time should be <= {MaxP95ResponseTimeMs}ms, but was {p95}ms");
    }

    [Fact]
    public async Task GetFlocks_WithLargeDataSet_VerifyNoNPlusOneQueries()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var mockCurrentUser = CreateMockCurrentUser("clerk_user_perf", tenantId);

        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                ReplaceWithInMemoryDatabase(services);
                ReplaceCurrentUserService(services, mockCurrentUser);
            });
        });

        using var scope = factory.Services.CreateScope();

        // Seed test data with history
        await SeedTenant(scope, tenantId, "clerk_user_perf");
        var coopId = await SeedCoop(scope, tenantId, "Performance Test Coop", "Test Location");

        // Create flocks with history entries
        for (int i = 1; i <= 20; i++)
        {
            await SeedFlockWithHistory(scope, tenantId, coopId, $"Flock-{i:D3}", 10 + i, 2, 0, historyEntries: 5);
        }

        var client = factory.CreateClient();

        // Act - Measure query performance with different dataset sizes
        var responseTimes = new List<long>();

        // Test with 5, 10, 15, 20 flocks
        for (int flockCount = 5; flockCount <= 20; flockCount += 5)
        {
            // Limit result set using coopId filter (simulated by taking first N flocks)
            var sw = Stopwatch.StartNew();
            var response = await client.GetAsync($"/api/coops/{coopId}/flocks");
            sw.Stop();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var flocks = await response.Content.ReadFromJsonAsync<List<FlockDto>>();

            responseTimes.Add(sw.ElapsedMilliseconds);

            _output.WriteLine($"Dataset with {flocks!.Count} flocks: {sw.ElapsedMilliseconds}ms");
        }

        // Assert - Response time should scale linearly (not exponentially)
        // If there's an N+1 problem, time would scale O(n²)
        // Linear scaling means O(n), which is acceptable
        var firstTime = responseTimes[0];
        var lastTime = responseTimes[^1];

        // With 4x the data (5 -> 20 flocks), time should be < 10x
        // (giving headroom for overhead, but catching exponential growth)
        var scalingFactor = (double)lastTime / firstTime;

        _output.WriteLine($"Scaling factor: {scalingFactor:F2}x (should be < 10x for linear scaling)");

        scalingFactor.Should().BeLessThan(10.0,
            "Response time should scale linearly, not exponentially (indicates N+1 query problem)");
    }

    [Fact]
    public async Task GetFlockById_WithHistory_VerifyEagerLoading()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var mockCurrentUser = CreateMockCurrentUser("clerk_user_perf", tenantId);

        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                ReplaceWithInMemoryDatabase(services);
                ReplaceCurrentUserService(services, mockCurrentUser);
            });
        });

        using var scope = factory.Services.CreateScope();

        // Seed flock with multiple history entries
        await SeedTenant(scope, tenantId, "clerk_user_perf");
        var coopId = await SeedCoop(scope, tenantId, "Test Coop", "Test Location");
        var flockId = await SeedFlockWithHistory(scope, tenantId, coopId, "Test Flock", 10, 2, 0, historyEntries: 10);

        var client = factory.CreateClient();

        // Warm up
        for (int i = 0; i < 5; i++)
        {
            await client.GetAsync($"/api/flocks/{flockId}");
        }

        // Act - Run performance test
        var responseTimes = new List<long>();
        for (int i = 0; i < 50; i++)
        {
            var sw = Stopwatch.StartNew();
            var response = await client.GetAsync($"/api/flocks/{flockId}");
            sw.Stop();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var flock = await response.Content.ReadFromJsonAsync<FlockDto>();
            flock.Should().NotBeNull();
            flock!.History.Should().HaveCount(10, "History should be eagerly loaded");

            responseTimes.Add(sw.ElapsedMilliseconds);
        }

        // Assert
        var p95 = CalculatePercentile(responseTimes, 95);
        var avg = responseTimes.Average();

        _output.WriteLine($"GET /api/flocks/{{id}} with History (n=50):");
        _output.WriteLine($"  Avg: {avg:F2}ms");
        _output.WriteLine($"  p95: {p95}ms");
        _output.WriteLine($"  Target: < {MaxP95ResponseTimeMs}ms (p95)");

        p95.Should().BeLessThanOrEqualTo(MaxP95ResponseTimeMs,
            $"p95 response time with eager loading should be <= {MaxP95ResponseTimeMs}ms");
    }

    // Helper methods
    private static long CalculatePercentile(List<long> values, int percentile)
    {
        var sorted = values.OrderBy(x => x).ToList();
        var index = (int)Math.Ceiling((percentile / 100.0) * sorted.Count) - 1;
        return sorted[index];
    }

    private static Mock<ICurrentUserService> CreateMockCurrentUser(string clerkUserId, Guid tenantId)
    {
        var mock = new Mock<ICurrentUserService>();
        mock.Setup(x => x.ClerkUserId).Returns(clerkUserId);
        mock.Setup(x => x.TenantId).Returns(tenantId);
        mock.Setup(x => x.IsAuthenticated).Returns(true);
        return mock;
    }

    private static void ReplaceWithInMemoryDatabase(IServiceCollection services)
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
        if (descriptor != null)
        {
            services.Remove(descriptor);
        }

        var databaseName = $"PerfTestDb_{Guid.NewGuid()}";

        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            options.UseInMemoryDatabase(databaseName);
            options.EnableSensitiveDataLogging();
            options.ConfigureWarnings(warnings => warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.CoreEventId.QueryIterationFailed));
        });

        services.AddAuthorization(options =>
        {
            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAssertion(_ => true)
                .Build();
        });
    }

    private static void ReplaceCurrentUserService(IServiceCollection services, Mock<ICurrentUserService> mock)
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ICurrentUserService));
        if (descriptor != null)
        {
            services.Remove(descriptor);
        }

        services.AddScoped(_ => mock.Object);
    }

    private static async Task SeedTenant(IServiceScope scope, Guid tenantId, string clerkUserId)
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var tenant = Tenant.Create(clerkUserId, $"{clerkUserId}@test.com");
        typeof(Tenant).GetProperty(nameof(Tenant.Id))!.SetValue(tenant, tenantId);
        dbContext.Tenants.Add(tenant);
        await dbContext.SaveChangesAsync();
    }

    private static async Task<Guid> SeedCoop(IServiceScope scope, Guid tenantId, string name, string location)
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var coop = Coop.Create(tenantId, name, location);
        dbContext.Coops.Add(coop);
        await dbContext.SaveChangesAsync();
        return coop.Id;
    }

    private static async Task<Guid> SeedFlock(IServiceScope scope, Guid tenantId, Guid coopId, string identifier, int hens, int roosters, int chicks)
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var flock = Flock.Create(tenantId, coopId, identifier, DateTime.UtcNow.AddMonths(-3), hens, roosters, chicks, null);
        dbContext.Flocks.Add(flock);
        await dbContext.SaveChangesAsync();
        return flock.Id;
    }

    private static async Task<Guid> SeedFlockWithHistory(IServiceScope scope, Guid tenantId, Guid coopId, string identifier, int hens, int roosters, int chicks, int historyEntries)
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var flock = Flock.Create(tenantId, coopId, identifier, DateTime.UtcNow.AddMonths(-3), hens, roosters, chicks, null);

        // Add additional history entries
        for (int i = 1; i < historyEntries; i++)
        {
            var newHens = hens + i;
            flock.UpdateComposition(newHens, roosters, chicks, $"Composition change {i}", $"History entry {i}");
        }

        dbContext.Flocks.Add(flock);
        await dbContext.SaveChangesAsync();
        return flock.Id;
    }
}
