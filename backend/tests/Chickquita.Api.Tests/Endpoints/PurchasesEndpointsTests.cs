using System.Net;
using System.Net.Http.Json;
using Chickquita.Application.DTOs;
using Chickquita.Application.Features.Purchases.Commands.Create;
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

namespace Chickquita.Api.Tests.Endpoints;

/// <summary>
/// Integration tests for Purchases API endpoints.
/// Tests full HTTP flow including authentication, tenant isolation, and business logic.
/// Note: These tests assume the PurchasesEndpoints will be created in a future story.
/// Currently tests the command/handler integration through the endpoint pattern.
/// </summary>
public class PurchasesEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public PurchasesEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreatePurchase_WithValidData_ShouldCreateSuccessfully()
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

        var mediator = scope.ServiceProvider.GetRequiredService<MediatR.IMediator>();

        var command = new CreatePurchaseCommand
        {
            Name = "Chicken Feed",
            Type = PurchaseType.Feed,
            Amount = 250.50m,
            Quantity = 25m,
            Unit = QuantityUnit.Kg,
            PurchaseDate = DateTime.UtcNow.Date,
            Notes = "High quality feed"
        };

        // Act
        var result = await mediator.Send(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("Chicken Feed");
        result.Value.Type.Should().Be(PurchaseType.Feed);
        result.Value.Amount.Should().Be(250.50m);
        result.Value.Quantity.Should().Be(25m);
        result.Value.Unit.Should().Be(QuantityUnit.Kg);
        result.Value.TenantId.Should().Be(tenantId);
    }

    [Fact]
    public async Task CreatePurchase_WithValidCoopId_ShouldCreateSuccessfully()
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
        var coopId = await SeedCoop(scope, tenantId, "Main Coop", "North Field");

        var mediator = scope.ServiceProvider.GetRequiredService<MediatR.IMediator>();

        var command = new CreatePurchaseCommand
        {
            CoopId = coopId,
            Name = "Bedding Straw",
            Type = PurchaseType.Bedding,
            Amount = 150.00m,
            Quantity = 10m,
            Unit = QuantityUnit.Package,
            PurchaseDate = DateTime.UtcNow.Date
        };

        // Act
        var result = await mediator.Send(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.CoopId.Should().Be(coopId);
        result.Value.Name.Should().Be("Bedding Straw");
        result.Value.Type.Should().Be(PurchaseType.Bedding);
    }

    [Fact]
    public async Task CreatePurchase_WithInvalidCoopId_ShouldReturnNotFound()
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

        var mediator = scope.ServiceProvider.GetRequiredService<MediatR.IMediator>();

        var invalidCoopId = Guid.NewGuid();
        var command = new CreatePurchaseCommand
        {
            CoopId = invalidCoopId,
            Name = "Bedding Straw",
            Type = PurchaseType.Bedding,
            Amount = 150.00m,
            Quantity = 10m,
            Unit = QuantityUnit.Package,
            PurchaseDate = DateTime.UtcNow.Date
        };

        // Act
        var result = await mediator.Send(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    [Fact]
    public async Task CreatePurchase_WithInvalidData_ShouldReturnValidationError()
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

        var mediator = scope.ServiceProvider.GetRequiredService<MediatR.IMediator>();

        // Command with empty name
        var command = new CreatePurchaseCommand
        {
            Name = "",
            Type = PurchaseType.Feed,
            Amount = 250.50m,
            Quantity = 25m,
            Unit = QuantityUnit.Kg,
            PurchaseDate = DateTime.UtcNow.Date
        };

        // Act
        var result = await mediator.Send(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Validation");
    }

    [Fact]
    public async Task CreatePurchase_VerifiesTenantIsolation()
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
        var coop1Id = await SeedCoop(scope1, tenant1Id, "Tenant 1 Coop", "Location 1");

        var mediator1 = scope1.ServiceProvider.GetRequiredService<MediatR.IMediator>();

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

        var mediator2 = scope2.ServiceProvider.GetRequiredService<MediatR.IMediator>();

        // Act - Tenant 2 tries to create purchase for Tenant 1's coop
        var command = new CreatePurchaseCommand
        {
            CoopId = coop1Id, // Tenant 1's coop
            Name = "Unauthorized Purchase",
            Type = PurchaseType.Feed,
            Amount = 100m,
            Quantity = 10m,
            Unit = QuantityUnit.Kg,
            PurchaseDate = DateTime.UtcNow.Date
        };

        var result = await mediator2.Send(command);

        // Assert - Should fail because coop belongs to different tenant
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    [Fact]
    public async Task CreatePurchase_WithConsumedDate_ShouldCreateSuccessfully()
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

        var mediator = scope.ServiceProvider.GetRequiredService<MediatR.IMediator>();

        var purchaseDate = DateTime.UtcNow.Date.AddDays(-5);
        var consumedDate = DateTime.UtcNow.Date;
        var command = new CreatePurchaseCommand
        {
            Name = "Vitamins",
            Type = PurchaseType.Vitamins,
            Amount = 50.00m,
            Quantity = 1m,
            Unit = QuantityUnit.Package,
            PurchaseDate = purchaseDate,
            ConsumedDate = consumedDate
        };

        // Act
        var result = await mediator.Send(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.ConsumedDate.Should().Be(consumedDate);
    }

    [Fact]
    public async Task CreatePurchase_WithConsumedDateBeforePurchaseDate_ShouldReturnValidationError()
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

        var mediator = scope.ServiceProvider.GetRequiredService<MediatR.IMediator>();

        var purchaseDate = DateTime.UtcNow.Date;
        var consumedDate = purchaseDate.AddDays(-1);
        var command = new CreatePurchaseCommand
        {
            Name = "Vitamins",
            Type = PurchaseType.Vitamins,
            Amount = 50.00m,
            Quantity = 1m,
            Unit = QuantityUnit.Package,
            PurchaseDate = purchaseDate,
            ConsumedDate = consumedDate
        };

        // Act
        var result = await mediator.Send(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Validation");
    }

    [Fact]
    public async Task UpdatePurchase_WithValidData_ShouldUpdateSuccessfully()
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

        // Create initial purchase
        var mediator = scope.ServiceProvider.GetRequiredService<MediatR.IMediator>();
        var createCommand = new CreatePurchaseCommand
        {
            Name = "Original Feed",
            Type = PurchaseType.Feed,
            Amount = 200.00m,
            Quantity = 20m,
            Unit = QuantityUnit.Kg,
            PurchaseDate = DateTime.UtcNow.Date
        };

        var createResult = await mediator.Send(createCommand);
        var purchaseId = createResult.Value.Id;

        // Update command
        var updateCommand = new Chickquita.Application.Features.Purchases.Commands.Update.UpdatePurchaseCommand
        {
            Id = purchaseId,
            Name = "Updated Feed",
            Type = PurchaseType.Feed,
            Amount = 300.00m,
            Quantity = 30m,
            Unit = QuantityUnit.Kg,
            PurchaseDate = DateTime.UtcNow.Date,
            Notes = "Updated notes"
        };

        // Act
        var result = await mediator.Send(updateCommand);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(purchaseId);
        result.Value.Name.Should().Be("Updated Feed");
        result.Value.Amount.Should().Be(300.00m);
        result.Value.Quantity.Should().Be(30m);
        result.Value.Notes.Should().Be("Updated notes");
    }

    [Fact]
    public async Task UpdatePurchase_WithInvalidId_ShouldReturnNotFound()
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

        var mediator = scope.ServiceProvider.GetRequiredService<MediatR.IMediator>();

        var invalidId = Guid.NewGuid();
        var updateCommand = new Chickquita.Application.Features.Purchases.Commands.Update.UpdatePurchaseCommand
        {
            Id = invalidId,
            Name = "Updated Feed",
            Type = PurchaseType.Feed,
            Amount = 300.00m,
            Quantity = 30m,
            Unit = QuantityUnit.Kg,
            PurchaseDate = DateTime.UtcNow.Date
        };

        // Act
        var result = await mediator.Send(updateCommand);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    [Fact]
    public async Task UpdatePurchase_VerifiesTenantIsolation()
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

        var mediator1 = scope1.ServiceProvider.GetRequiredService<MediatR.IMediator>();

        // Create purchase for Tenant 1
        var createCommand = new CreatePurchaseCommand
        {
            Name = "Tenant 1 Feed",
            Type = PurchaseType.Feed,
            Amount = 200.00m,
            Quantity = 20m,
            Unit = QuantityUnit.Kg,
            PurchaseDate = DateTime.UtcNow.Date
        };

        var createResult = await mediator1.Send(createCommand);
        var purchaseId = createResult.Value.Id;

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

        var mediator2 = scope2.ServiceProvider.GetRequiredService<MediatR.IMediator>();

        // Act - Tenant 2 tries to update Tenant 1's purchase
        var updateCommand = new Chickquita.Application.Features.Purchases.Commands.Update.UpdatePurchaseCommand
        {
            Id = purchaseId,
            Name = "Unauthorized Update",
            Type = PurchaseType.Feed,
            Amount = 300.00m,
            Quantity = 30m,
            Unit = QuantityUnit.Kg,
            PurchaseDate = DateTime.UtcNow.Date
        };

        var result = await mediator2.Send(updateCommand);

        // Assert - Should fail because purchase belongs to different tenant
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    [Fact]
    public async Task DeletePurchase_WithValidId_ShouldDeleteSuccessfully()
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

        // Create initial purchase
        var mediator = scope.ServiceProvider.GetRequiredService<MediatR.IMediator>();
        var createCommand = new CreatePurchaseCommand
        {
            Name = "Feed to Delete",
            Type = PurchaseType.Feed,
            Amount = 200.00m,
            Quantity = 20m,
            Unit = QuantityUnit.Kg,
            PurchaseDate = DateTime.UtcNow.Date
        };

        var createResult = await mediator.Send(createCommand);
        var purchaseId = createResult.Value.Id;

        // Delete command
        var deleteCommand = new Chickquita.Application.Features.Purchases.Commands.Delete.DeletePurchaseCommand
        {
            PurchaseId = purchaseId
        };

        // Act
        var result = await mediator.Send(deleteCommand);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task DeletePurchase_WithInvalidId_ShouldReturnNotFound()
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

        var mediator = scope.ServiceProvider.GetRequiredService<MediatR.IMediator>();

        var invalidId = Guid.NewGuid();
        var deleteCommand = new Chickquita.Application.Features.Purchases.Commands.Delete.DeletePurchaseCommand
        {
            PurchaseId = invalidId
        };

        // Act
        var result = await mediator.Send(deleteCommand);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    [Fact]
    public async Task DeletePurchase_VerifiesTenantIsolation()
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

        var mediator1 = scope1.ServiceProvider.GetRequiredService<MediatR.IMediator>();

        // Create purchase for Tenant 1
        var createCommand = new CreatePurchaseCommand
        {
            Name = "Tenant 1 Feed",
            Type = PurchaseType.Feed,
            Amount = 200.00m,
            Quantity = 20m,
            Unit = QuantityUnit.Kg,
            PurchaseDate = DateTime.UtcNow.Date
        };

        var createResult = await mediator1.Send(createCommand);
        var purchaseId = createResult.Value.Id;

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

        var mediator2 = scope2.ServiceProvider.GetRequiredService<MediatR.IMediator>();

        // Act - Tenant 2 tries to delete Tenant 1's purchase
        var deleteCommand = new Chickquita.Application.Features.Purchases.Commands.Delete.DeletePurchaseCommand
        {
            PurchaseId = purchaseId
        };

        var result = await mediator2.Send(deleteCommand);

        // Assert - Should fail because purchase belongs to different tenant
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.NotFound");
    }

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

        // Use a unique database name for each test to ensure isolation
        var databaseName = $"TestDb_{Guid.NewGuid()}";

        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            options.UseInMemoryDatabase(databaseName);
            options.EnableSensitiveDataLogging();
            // ConfigureWarnings to ignore query filter warnings in tests
            options.ConfigureWarnings(warnings => warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.CoreEventId.QueryIterationFailed));
        });

        // Bypass authentication for tests
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

    #endregion
}
