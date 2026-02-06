using System.Net;
using System.Security.Claims;
using Chickquita.Application.Interfaces;
using Chickquita.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Chickquita.Api.Tests.Middleware;

/// <summary>
/// Tests for TenantResolutionMiddleware.
/// Following TDD principles: Write tests first, watch them fail, then implement.
/// </summary>
public class TenantResolutionMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WithValidClerkUserId_ExtractsClerkUserIdFromJWT()
    {
        // Arrange
        var clerkUserId = "user_123";
        var tenantId = Guid.NewGuid();
        var tenant = Tenant.Create(clerkUserId, "test@example.com");

        var mockTenantRepository = new Mock<ITenantRepository>();
        mockTenantRepository
            .Setup(r => r.GetByClerkUserIdAsync(clerkUserId))
            .ReturnsAsync(tenant);

        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("sub", clerkUserId)
        }, "TestAuth"));

        var requestDelegate = new Mock<RequestDelegate>();
        requestDelegate
            .Setup(rd => rd(It.IsAny<HttpContext>()))
            .Returns(Task.CompletedTask);

        var mockLogger = new Mock<ILogger<Chickquita.Api.Middleware.TenantResolutionMiddleware>>();

        var middleware = new Chickquita.Api.Middleware.TenantResolutionMiddleware(
            requestDelegate.Object);

        // Act
        await middleware.InvokeAsync(httpContext, mockTenantRepository.Object, mockLogger.Object);

        // Assert
        mockTenantRepository.Verify(
            r => r.GetByClerkUserIdAsync(clerkUserId),
            Times.Once,
            "Middleware should extract Clerk user ID from JWT and use it to fetch tenant");
    }

    [Fact]
    public async Task InvokeAsync_WithValidTenant_StoresTenantIdInHttpContextItems()
    {
        // Arrange
        var clerkUserId = "user_123";
        var tenant = Tenant.Create(clerkUserId, "test@example.com");

        var mockTenantRepository = new Mock<ITenantRepository>();
        mockTenantRepository
            .Setup(r => r.GetByClerkUserIdAsync(clerkUserId))
            .ReturnsAsync(tenant);

        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("sub", clerkUserId)
        }, "TestAuth"));

        var requestDelegate = new Mock<RequestDelegate>();
        requestDelegate
            .Setup(rd => rd(It.IsAny<HttpContext>()))
            .Returns(Task.CompletedTask);

        var mockLogger = new Mock<ILogger<Chickquita.Api.Middleware.TenantResolutionMiddleware>>();

        var middleware = new Chickquita.Api.Middleware.TenantResolutionMiddleware(
            requestDelegate.Object);

        // Act
        await middleware.InvokeAsync(httpContext, mockTenantRepository.Object, mockLogger.Object);

        // Assert
        httpContext.Items.Should().ContainKey("TenantId");
        httpContext.Items["TenantId"].Should().Be(tenant.Id);
    }

    [Fact]
    public async Task InvokeAsync_WhenTenantNotFound_AutoCreatesNewTenant()
    {
        // Arrange
        var clerkUserId = "user_123";
        var email = "test@example.com";
        var createdTenant = Tenant.Create(clerkUserId, email);

        var mockTenantRepository = new Mock<ITenantRepository>();
        mockTenantRepository
            .Setup(r => r.GetByClerkUserIdAsync(clerkUserId))
            .ReturnsAsync((Tenant?)null);
        mockTenantRepository
            .Setup(r => r.AddAsync(It.IsAny<Tenant>()))
            .ReturnsAsync(createdTenant);

        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("sub", clerkUserId),
            new Claim("email", email)
        }, "TestAuth"));

        var requestDelegate = new Mock<RequestDelegate>();
        requestDelegate
            .Setup(rd => rd(It.IsAny<HttpContext>()))
            .Returns(Task.CompletedTask);

        var mockLogger = new Mock<ILogger<Chickquita.Api.Middleware.TenantResolutionMiddleware>>();

        var middleware = new Chickquita.Api.Middleware.TenantResolutionMiddleware(
            requestDelegate.Object);

        // Act
        await middleware.InvokeAsync(httpContext, mockTenantRepository.Object, mockLogger.Object);

        // Assert
        mockTenantRepository.Verify(
            r => r.AddAsync(It.Is<Tenant>(t => t.ClerkUserId == clerkUserId && t.Email == email)),
            Times.Once,
            "Should auto-create tenant when not found");

        httpContext.Items.Should().ContainKey("TenantId");
        httpContext.Items["TenantId"].Should().Be(createdTenant.Id);

        requestDelegate.Verify(
            rd => rd(It.IsAny<HttpContext>()),
            Times.Once,
            "Should call next middleware after creating tenant");
    }

    [Fact]
    public async Task InvokeAsync_WithValidTenant_CallsNextMiddleware()
    {
        // Arrange
        var clerkUserId = "user_123";
        var tenant = Tenant.Create(clerkUserId, "test@example.com");

        var mockTenantRepository = new Mock<ITenantRepository>();
        mockTenantRepository
            .Setup(r => r.GetByClerkUserIdAsync(clerkUserId))
            .ReturnsAsync(tenant);

        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("sub", clerkUserId)
        }, "TestAuth"));

        var nextCalled = false;
        var requestDelegate = new Mock<RequestDelegate>();
        requestDelegate
            .Setup(rd => rd(It.IsAny<HttpContext>()))
            .Callback(() => nextCalled = true)
            .Returns(Task.CompletedTask);

        var mockLogger = new Mock<ILogger<Chickquita.Api.Middleware.TenantResolutionMiddleware>>();

        var middleware = new Chickquita.Api.Middleware.TenantResolutionMiddleware(
            requestDelegate.Object);

        // Act
        await middleware.InvokeAsync(httpContext, mockTenantRepository.Object, mockLogger.Object);

        // Assert
        nextCalled.Should().BeTrue("Middleware should call next delegate when tenant is found");
    }

    [Fact]
    public async Task InvokeAsync_WhenUserNotAuthenticated_CallsNextMiddlewareWithoutSettingTenant()
    {
        // Arrange
        var mockTenantRepository = new Mock<ITenantRepository>();

        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity()); // Not authenticated

        var nextCalled = false;
        var requestDelegate = new Mock<RequestDelegate>();
        requestDelegate
            .Setup(rd => rd(It.IsAny<HttpContext>()))
            .Callback(() => nextCalled = true)
            .Returns(Task.CompletedTask);

        var mockLogger = new Mock<ILogger<Chickquita.Api.Middleware.TenantResolutionMiddleware>>();

        var middleware = new Chickquita.Api.Middleware.TenantResolutionMiddleware(
            requestDelegate.Object);

        // Act
        await middleware.InvokeAsync(httpContext, mockTenantRepository.Object, mockLogger.Object);

        // Assert
        nextCalled.Should().BeTrue("Middleware should call next delegate for unauthenticated requests");
        httpContext.Items.Should().NotContainKey("TenantId");
        mockTenantRepository.Verify(
            r => r.GetByClerkUserIdAsync(It.IsAny<string>()),
            Times.Never,
            "Should not query tenant repository when user is not authenticated");
    }

    [Fact]
    public async Task InvokeAsync_WhenTenantNotFound_AndNoEmailInClaims_UsesFallbackEmail()
    {
        // Arrange
        var clerkUserId = "user_123";
        var expectedFallbackEmail = $"{clerkUserId}@clerk.temp";
        var createdTenant = Tenant.Create(clerkUserId, expectedFallbackEmail);

        var mockTenantRepository = new Mock<ITenantRepository>();
        mockTenantRepository
            .Setup(r => r.GetByClerkUserIdAsync(clerkUserId))
            .ReturnsAsync((Tenant?)null);
        mockTenantRepository
            .Setup(r => r.AddAsync(It.IsAny<Tenant>()))
            .ReturnsAsync(createdTenant);

        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("sub", clerkUserId)
            // No email claim
        }, "TestAuth"));

        var requestDelegate = new Mock<RequestDelegate>();
        requestDelegate
            .Setup(rd => rd(It.IsAny<HttpContext>()))
            .Returns(Task.CompletedTask);

        var mockLogger = new Mock<ILogger<Chickquita.Api.Middleware.TenantResolutionMiddleware>>();

        var middleware = new Chickquita.Api.Middleware.TenantResolutionMiddleware(
            requestDelegate.Object);

        // Act
        await middleware.InvokeAsync(httpContext, mockTenantRepository.Object, mockLogger.Object);

        // Assert
        mockTenantRepository.Verify(
            r => r.AddAsync(It.Is<Tenant>(t =>
                t.ClerkUserId == clerkUserId &&
                t.Email == expectedFallbackEmail)),
            Times.Once,
            "Should use fallback email when email claim is not present");
    }
}
