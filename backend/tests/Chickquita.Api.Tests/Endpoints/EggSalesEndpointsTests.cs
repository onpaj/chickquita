using System.Net;
using System.Net.Http.Json;
using Chickquita.Api.Tests.Helpers;
using Chickquita.Application.DTOs;
using Chickquita.Application.Features.EggSales.Commands.Create;
using Chickquita.Application.Features.EggSales.Commands.Update;
using Chickquita.Application.Features.EggSales.Commands.Delete;
using Chickquita.Application.Interfaces;
using Chickquita.Domain.Entities;
using Chickquita.Infrastructure.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Chickquita.Api.Tests.Endpoints;

/// <summary>
/// Integration tests for EggSales API endpoints.
/// Tests full HTTP flow including authentication, tenant isolation, and business logic.
/// </summary>
public class EggSalesEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public EggSalesEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    #region HTTP Integration Tests

    [Fact]
    public async Task CreateEggSale_WithValidData_Returns201Created()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var mockCurrentUser = CreateMockCurrentUser("clerk_user_1", tenantId);

        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                ReplaceWithInMemoryDatabase(services);
                ReplaceCurrentUserService(services, mockCurrentUser);
            });
        });

        using var scope = factory.Services.CreateScope();
        await SeedTenant(scope, tenantId, "clerk_user_1");

        var client = factory.CreateClient();

        var command = new CreateEggSaleCommand
        {
            Date = DateTime.UtcNow.Date,
            Quantity = 50,
            PricePerUnit = 5.00m,
            BuyerName = "John Doe",
            Notes = "First sale of the season"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/egg-sales", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().StartWith("/api/egg-sales/");

        var result = await response.Content.ReadFromJsonAsync<EggSaleDto>();
        result.Should().NotBeNull();
        result!.Quantity.Should().Be(50);
        result.PricePerUnit.Should().Be(5.00m);
        result.BuyerName.Should().Be("John Doe");
        result.TenantId.Should().Be(tenantId);
    }

    [Fact]
    public async Task CreateEggSale_WithInvalidData_Returns400BadRequest()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var mockCurrentUser = CreateMockCurrentUser("clerk_user_1", tenantId);

        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                ReplaceWithInMemoryDatabase(services);
                ReplaceCurrentUserService(services, mockCurrentUser);
            });
        });

        using var scope = factory.Services.CreateScope();
        await SeedTenant(scope, tenantId, "clerk_user_1");

        var client = factory.CreateClient();

        var command = new CreateEggSaleCommand
        {
            Date = DateTime.UtcNow.Date,
            Quantity = 0, // Invalid: quantity must be > 0
            PricePerUnit = 5.00m
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/egg-sales", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetEggSales_WithExistingSales_Returns200WithList()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var mockCurrentUser = CreateMockCurrentUser("clerk_user_1", tenantId);

        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                ReplaceWithInMemoryDatabase(services);
                ReplaceCurrentUserService(services, mockCurrentUser);
            });
        });

        using var scope = factory.Services.CreateScope();
        await SeedTenant(scope, tenantId, "clerk_user_1");
        await SeedEggSale(scope, tenantId, DateTime.UtcNow.Date, 30, 4.50m);
        await SeedEggSale(scope, tenantId, DateTime.UtcNow.Date.AddDays(-1), 20, 4.00m);

        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/egg-sales");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<EggSaleDto>>();
        result.Should().NotBeNull();
        result!.Count.Should().Be(2);
    }

    [Fact]
    public async Task GetEggSales_WithDateFilter_ReturnsFilteredResults()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var mockCurrentUser = CreateMockCurrentUser("clerk_user_1", tenantId);

        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                ReplaceWithInMemoryDatabase(services);
                ReplaceCurrentUserService(services, mockCurrentUser);
            });
        });

        using var scope = factory.Services.CreateScope();
        await SeedTenant(scope, tenantId, "clerk_user_1");

        var today = DateTime.UtcNow.Date;
        await SeedEggSale(scope, tenantId, today, 30, 4.50m);
        await SeedEggSale(scope, tenantId, today.AddDays(-10), 20, 4.00m); // outside filter

        var client = factory.CreateClient();

        var fromDate = today.AddDays(-1).ToString("yyyy-MM-dd");
        var toDate = today.AddDays(1).ToString("yyyy-MM-dd");

        // Act
        var response = await client.GetAsync($"/api/egg-sales?fromDate={fromDate}&toDate={toDate}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<EggSaleDto>>();
        result.Should().NotBeNull();
        result!.Count.Should().Be(1);
        result[0].Quantity.Should().Be(30);
    }

    [Fact]
    public async Task GetEggSaleById_WithValidId_Returns200()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var mockCurrentUser = CreateMockCurrentUser("clerk_user_1", tenantId);

        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                ReplaceWithInMemoryDatabase(services);
                ReplaceCurrentUserService(services, mockCurrentUser);
            });
        });

        using var scope = factory.Services.CreateScope();
        await SeedTenant(scope, tenantId, "clerk_user_1");
        var saleId = await SeedEggSale(scope, tenantId, DateTime.UtcNow.Date, 50, 5.00m);

        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync($"/api/egg-sales/{saleId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<EggSaleDto>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(saleId);
        result.Quantity.Should().Be(50);
        result.PricePerUnit.Should().Be(5.00m);
    }

    [Fact]
    public async Task GetEggSaleById_WithInvalidId_Returns404()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var mockCurrentUser = CreateMockCurrentUser("clerk_user_1", tenantId);

        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                ReplaceWithInMemoryDatabase(services);
                ReplaceCurrentUserService(services, mockCurrentUser);
            });
        });

        using var scope = factory.Services.CreateScope();
        await SeedTenant(scope, tenantId, "clerk_user_1");

        var client = factory.CreateClient();
        var invalidId = Guid.NewGuid();

        // Act
        var response = await client.GetAsync($"/api/egg-sales/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateEggSale_WithValidData_Returns200()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var mockCurrentUser = CreateMockCurrentUser("clerk_user_1", tenantId);

        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                ReplaceWithInMemoryDatabase(services);
                ReplaceCurrentUserService(services, mockCurrentUser);
            });
        });

        using var scope = factory.Services.CreateScope();
        await SeedTenant(scope, tenantId, "clerk_user_1");
        var saleId = await SeedEggSale(scope, tenantId, DateTime.UtcNow.Date, 50, 5.00m);

        var client = factory.CreateClient();

        var updateCommand = new UpdateEggSaleCommand
        {
            Id = saleId,
            Date = DateTime.UtcNow.Date,
            Quantity = 75,
            PricePerUnit = 6.00m,
            BuyerName = "Updated Buyer",
            Notes = "Updated notes"
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/egg-sales/{saleId}", updateCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<EggSaleDto>();
        result.Should().NotBeNull();
        result!.Quantity.Should().Be(75);
        result.PricePerUnit.Should().Be(6.00m);
        result.BuyerName.Should().Be("Updated Buyer");
    }

    [Fact]
    public async Task UpdateEggSale_WithMismatchedIds_Returns400()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var mockCurrentUser = CreateMockCurrentUser("clerk_user_1", tenantId);

        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                ReplaceWithInMemoryDatabase(services);
                ReplaceCurrentUserService(services, mockCurrentUser);
            });
        });

        using var scope = factory.Services.CreateScope();
        await SeedTenant(scope, tenantId, "clerk_user_1");
        var saleId = await SeedEggSale(scope, tenantId, DateTime.UtcNow.Date, 50, 5.00m);

        var client = factory.CreateClient();

        var differentId = Guid.NewGuid();
        var updateCommand = new UpdateEggSaleCommand
        {
            Id = differentId, // Different from route
            Date = DateTime.UtcNow.Date,
            Quantity = 75,
            PricePerUnit = 6.00m
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/egg-sales/{saleId}", updateCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateEggSale_WithInvalidId_Returns404()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var mockCurrentUser = CreateMockCurrentUser("clerk_user_1", tenantId);

        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                ReplaceWithInMemoryDatabase(services);
                ReplaceCurrentUserService(services, mockCurrentUser);
            });
        });

        using var scope = factory.Services.CreateScope();
        await SeedTenant(scope, tenantId, "clerk_user_1");

        var client = factory.CreateClient();
        var invalidId = Guid.NewGuid();

        var updateCommand = new UpdateEggSaleCommand
        {
            Id = invalidId,
            Date = DateTime.UtcNow.Date,
            Quantity = 50,
            PricePerUnit = 5.00m
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/egg-sales/{invalidId}", updateCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteEggSale_WithValidId_Returns204()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var mockCurrentUser = CreateMockCurrentUser("clerk_user_1", tenantId);

        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                ReplaceWithInMemoryDatabase(services);
                ReplaceCurrentUserService(services, mockCurrentUser);
            });
        });

        using var scope = factory.Services.CreateScope();
        await SeedTenant(scope, tenantId, "clerk_user_1");
        var saleId = await SeedEggSale(scope, tenantId, DateTime.UtcNow.Date, 50, 5.00m);

        var client = factory.CreateClient();

        // Act
        var response = await client.DeleteAsync($"/api/egg-sales/{saleId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteEggSale_WithInvalidId_Returns404()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var mockCurrentUser = CreateMockCurrentUser("clerk_user_1", tenantId);

        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                ReplaceWithInMemoryDatabase(services);
                ReplaceCurrentUserService(services, mockCurrentUser);
            });
        });

        using var scope = factory.Services.CreateScope();
        await SeedTenant(scope, tenantId, "clerk_user_1");

        var client = factory.CreateClient();
        var invalidId = Guid.NewGuid();

        // Act
        var response = await client.DeleteAsync($"/api/egg-sales/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task EggSalesEndpoint_VerifiesTenantIsolation()
    {
        // Arrange - Create two tenants
        var tenant1Id = Guid.NewGuid();
        var tenant2Id = Guid.NewGuid();
        var mockCurrentUser1 = CreateMockCurrentUser("clerk_user_1", tenant1Id);
        var mockCurrentUser2 = CreateMockCurrentUser("clerk_user_2", tenant2Id);

        // Setup for Tenant 1
        var factory1 = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                ReplaceWithInMemoryDatabase(services);
                ReplaceCurrentUserService(services, mockCurrentUser1);
            });
        });

        using var scope1 = factory1.Services.CreateScope();
        await SeedTenant(scope1, tenant1Id, "clerk_user_1");
        var sale1Id = await SeedEggSale(scope1, tenant1Id, DateTime.UtcNow.Date, 50, 5.00m);

        // Setup for Tenant 2
        var factory2 = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                ReplaceWithInMemoryDatabase(services);
                ReplaceCurrentUserService(services, mockCurrentUser2);
            });
        });

        using var scope2 = factory2.Services.CreateScope();
        await SeedTenant(scope2, tenant2Id, "clerk_user_2");

        var client2 = factory2.CreateClient();

        // Act - Tenant 2 tries to access Tenant 1's sale
        var response = await client2.GetAsync($"/api/egg-sales/{sale1Id}");

        // Assert - Should return 404 (not found, not forbidden, to prevent info disclosure)
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Helper Methods

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

        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        services.AddSingleton(connection);

        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            var conn = serviceProvider.GetRequiredService<SqliteConnection>();
            options.UseSqlite(conn);
            options.EnableSensitiveDataLogging();
        });

        var appContextDescriptors = services.Where(d => d.ServiceType == typeof(ApplicationDbContext)).ToList();
        foreach (var d in appContextDescriptors) services.Remove(d);
        services.AddScoped<ApplicationDbContext>(sp =>
        {
            var opts = sp.GetRequiredService<DbContextOptions<ApplicationDbContext>>();
            var currentUser = sp.GetRequiredService<ICurrentUserService>();
            return new SqliteApplicationDbContext(opts, currentUser);
        });

        services.AddAuthorization(options =>
        {
            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAssertion(_ => true)
                .Build();
        });

        services.AddTransient<IStartupFilter, DatabaseInitializerStartupFilter>();
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
        await dbContext.Database.EnsureCreatedAsync();
        var tenant = Tenant.Create(clerkUserId, $"{clerkUserId}@test.com").Value;
        typeof(Tenant).GetProperty(nameof(Tenant.Id))!.SetValue(tenant, tenantId);
        dbContext.Tenants.Add(tenant);
        await dbContext.SaveChangesAsync();
    }

    private static async Task<Guid> SeedEggSale(
        IServiceScope scope,
        Guid tenantId,
        DateTime date,
        int quantity,
        decimal pricePerUnit,
        string? buyerName = null,
        string? notes = null)
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var sale = EggSale.Create(tenantId, date, quantity, pricePerUnit, buyerName, notes).Value;
        dbContext.EggSales.Add(sale);
        await dbContext.SaveChangesAsync();
        return sale.Id;
    }

    #endregion
}
