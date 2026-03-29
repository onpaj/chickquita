using AutoFixture;
using AutoFixture.AutoMoq;
using Chickquita.Application.DTOs;
using Chickquita.Application.Features.Settings.Queries;
using Chickquita.Application.Interfaces;
using Chickquita.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Chickquita.Application.Tests.Features.Settings.Queries;

/// <summary>
/// Unit tests for GetTenantSettingsQueryHandler.
/// Tests cover happy path, missing tenant, and unauthorized access.
/// </summary>
public class GetTenantSettingsQueryHandlerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<ITenantRepository> _mockTenantRepository;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<ILogger<GetTenantSettingsQueryHandler>> _mockLogger;
    private readonly GetTenantSettingsQueryHandler _handler;

    public GetTenantSettingsQueryHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());

        _mockTenantRepository = _fixture.Freeze<Mock<ITenantRepository>>();
        _mockCurrentUserService = _fixture.Freeze<Mock<ICurrentUserService>>();
        _mockLogger = _fixture.Freeze<Mock<ILogger<GetTenantSettingsQueryHandler>>>();

        _handler = new GetTenantSettingsQueryHandler(
            _mockTenantRepository.Object,
            _mockCurrentUserService.Object,
            _mockLogger.Object);
    }

    #region Happy Path Tests

    [Fact]
    public async Task Handle_WithValidTenant_ShouldReturnSettings()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var tenant = Tenant.Create("org_abc", "Test Farm").Value;
        typeof(Tenant).GetProperty(nameof(Tenant.Id))!.SetValue(tenant, tenantId);

        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockTenantRepository.Setup(x => x.GetByIdAsync(tenantId)).ReturnsAsync(tenant);

        // Act
        var result = await _handler.Handle(new GetTenantSettingsQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeOfType<TenantSettingsDto>();
    }

    [Fact]
    public async Task Handle_WithDefaultTenant_ShouldReturnRevenueTrackingEnabledTrue()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var tenant = Tenant.Create("org_abc", "Test Farm").Value;
        typeof(Tenant).GetProperty(nameof(Tenant.Id))!.SetValue(tenant, tenantId);

        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockTenantRepository.Setup(x => x.GetByIdAsync(tenantId)).ReturnsAsync(tenant);

        // Act
        var result = await _handler.Handle(new GetTenantSettingsQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.RevenueTrackingEnabled.Should().BeTrue();
        result.Value.Currency.Should().Be("CZK");
    }

    [Fact]
    public async Task Handle_WhenRevenueTrackingIsDisabled_ShouldReturnDisabledInDto()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var tenant = Tenant.Create("org_abc", "Test Farm").Value;
        typeof(Tenant).GetProperty(nameof(Tenant.Id))!.SetValue(tenant, tenantId);
        tenant.UpdateSettings(singleCoopMode: false, revenueTrackingEnabled: false, currency: "EUR");

        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockTenantRepository.Setup(x => x.GetByIdAsync(tenantId)).ReturnsAsync(tenant);

        // Act
        var result = await _handler.Handle(new GetTenantSettingsQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.RevenueTrackingEnabled.Should().BeFalse();
        result.Value.Currency.Should().Be("EUR");
        result.Value.SingleCoopMode.Should().BeFalse();
    }

    #endregion

    #region Authorization Tests

    [Fact]
    public async Task Handle_WhenTenantIdIsNull_ShouldReturnUnauthorizedError()
    {
        // Arrange
        _mockCurrentUserService.Setup(x => x.TenantId).Returns((Guid?)null);

        // Act
        var result = await _handler.Handle(new GetTenantSettingsQuery(), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Unauthorized");

        _mockTenantRepository.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    #endregion

    #region Not Found Tests

    [Fact]
    public async Task Handle_WhenTenantNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockTenantRepository.Setup(x => x.GetByIdAsync(tenantId)).ReturnsAsync((Tenant?)null);

        // Act
        var result = await _handler.Handle(new GetTenantSettingsQuery(), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldReturnFailureError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockTenantRepository.Setup(x => x.GetByIdAsync(tenantId))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(new GetTenantSettingsQuery(), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Failure");
        result.Error.Message.Should().Be("An unexpected error occurred. Please try again.");
    }

    #endregion
}
