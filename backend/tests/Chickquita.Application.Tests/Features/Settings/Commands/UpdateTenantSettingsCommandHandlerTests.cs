using AutoFixture;
using AutoFixture.AutoMoq;
using Chickquita.Application.Features.Settings.Commands;
using Chickquita.Application.Interfaces;
using Chickquita.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Chickquita.Application.Tests.Features.Settings.Commands;

/// <summary>
/// Unit tests for UpdateTenantSettingsCommandHandler.
/// Tests cover happy path, missing tenant, unauthorized access, and error handling.
/// </summary>
public class UpdateTenantSettingsCommandHandlerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<ITenantRepository> _mockTenantRepository;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ILogger<UpdateTenantSettingsCommandHandler>> _mockLogger;
    private readonly UpdateTenantSettingsCommandHandler _handler;

    public UpdateTenantSettingsCommandHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());

        _mockTenantRepository = _fixture.Freeze<Mock<ITenantRepository>>();
        _mockCurrentUserService = _fixture.Freeze<Mock<ICurrentUserService>>();
        _mockUnitOfWork = _fixture.Freeze<Mock<IUnitOfWork>>();
        _mockLogger = _fixture.Freeze<Mock<ILogger<UpdateTenantSettingsCommandHandler>>>();
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _handler = new UpdateTenantSettingsCommandHandler(
            _mockTenantRepository.Object,
            _mockCurrentUserService.Object,
            _mockUnitOfWork.Object,
            _mockLogger.Object);
    }

    #region Happy Path Tests

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateSettingsSuccessfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var tenant = Tenant.Create("org_abc", "Test Farm").Value;
        typeof(Tenant).GetProperty(nameof(Tenant.Id))!.SetValue(tenant, tenantId);

        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockTenantRepository.Setup(x => x.GetByIdAsync(tenantId)).ReturnsAsync(tenant);

        var command = new UpdateTenantSettingsCommand
        {
            SingleCoopMode = false,
            RevenueTrackingEnabled = true,
            Currency = "EUR"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        tenant.RevenueTrackingEnabled.Should().BeTrue();
        tenant.SingleCoopMode.Should().BeFalse();
        tenant.Currency.Should().Be("EUR");

        _mockTenantRepository.Verify(x => x.UpdateAsync(tenant), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithRevenueTrackingDisabled_ShouldPersistDisabledState()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var tenant = Tenant.Create("org_abc", "Test Farm").Value;
        typeof(Tenant).GetProperty(nameof(Tenant.Id))!.SetValue(tenant, tenantId);

        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockTenantRepository.Setup(x => x.GetByIdAsync(tenantId)).ReturnsAsync(tenant);

        var command = new UpdateTenantSettingsCommand
        {
            SingleCoopMode = true,
            RevenueTrackingEnabled = false,
            Currency = "CZK"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        tenant.RevenueTrackingEnabled.Should().BeFalse();

        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNullCurrency_ShouldDefaultToCZK()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var tenant = Tenant.Create("org_abc", "Test Farm").Value;
        typeof(Tenant).GetProperty(nameof(Tenant.Id))!.SetValue(tenant, tenantId);

        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockTenantRepository.Setup(x => x.GetByIdAsync(tenantId)).ReturnsAsync(tenant);

        var command = new UpdateTenantSettingsCommand
        {
            SingleCoopMode = false,
            RevenueTrackingEnabled = true,
            Currency = null
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        tenant.Currency.Should().Be("CZK");
    }

    #endregion

    #region Authorization Tests

    [Fact]
    public async Task Handle_WhenTenantIdIsNull_ShouldReturnUnauthorizedError()
    {
        // Arrange
        _mockCurrentUserService.Setup(x => x.TenantId).Returns((Guid?)null);

        var command = new UpdateTenantSettingsCommand
        {
            SingleCoopMode = false,
            RevenueTrackingEnabled = true
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Unauthorized");

        _mockTenantRepository.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
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

        var command = new UpdateTenantSettingsCommand
        {
            SingleCoopMode = false,
            RevenueTrackingEnabled = true
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");

        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
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

        var command = new UpdateTenantSettingsCommand
        {
            SingleCoopMode = false,
            RevenueTrackingEnabled = true
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Failure");
        result.Error.Message.Should().Be("An unexpected error occurred. Please try again.");
    }

    #endregion
}
