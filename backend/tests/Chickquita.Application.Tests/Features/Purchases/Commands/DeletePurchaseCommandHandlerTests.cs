using AutoFixture;
using AutoFixture.AutoMoq;
using Chickquita.Application.Features.Purchases.Commands.Delete;
using Chickquita.Application.Interfaces;
using Chickquita.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Chickquita.Application.Tests.Features.Purchases.Commands;

/// <summary>
/// Unit tests for DeletePurchaseCommandHandler.
/// Tests cover happy path, validation, tenant ownership, and error handling.
/// </summary>
public class DeletePurchaseCommandHandlerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IPurchaseRepository> _mockPurchaseRepository;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<ILogger<DeletePurchaseCommandHandler>> _mockLogger;
    private readonly DeletePurchaseCommandHandler _handler;

    public DeletePurchaseCommandHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());

        _mockPurchaseRepository = _fixture.Freeze<Mock<IPurchaseRepository>>();
        _mockCurrentUserService = _fixture.Freeze<Mock<ICurrentUserService>>();
        _mockLogger = _fixture.Freeze<Mock<ILogger<DeletePurchaseCommandHandler>>>();

        _handler = new DeletePurchaseCommandHandler(
            _mockPurchaseRepository.Object,
            _mockCurrentUserService.Object,
            _mockLogger.Object);
    }

    #region Happy Path Tests

    [Fact]
    public async Task Handle_WithValidPurchaseId_ShouldDeleteSuccessfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var purchaseId = Guid.NewGuid();
        var command = new DeletePurchaseCommand
        {
            PurchaseId = purchaseId
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        var existingPurchase = Purchase.Create(
            tenantId,
            "Chicken Feed",
            PurchaseType.Feed,
            250.50m,
            25m,
            QuantityUnit.Kg,
            DateTime.UtcNow.Date);

        _mockPurchaseRepository.Setup(x => x.GetByIdAsync(purchaseId))
            .ReturnsAsync(existingPurchase);

        _mockPurchaseRepository.Setup(x => x.DeleteAsync(purchaseId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();

        _mockPurchaseRepository.Verify(x => x.GetByIdAsync(purchaseId), Times.Once);
        _mockPurchaseRepository.Verify(x => x.DeleteAsync(purchaseId), Times.Once);
    }

    #endregion

    #region Not Found Tests

    [Fact]
    public async Task Handle_WithNonExistentPurchase_ShouldReturnNotFoundError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var purchaseId = Guid.NewGuid();
        var command = new DeletePurchaseCommand
        {
            PurchaseId = purchaseId
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockPurchaseRepository.Setup(x => x.GetByIdAsync(purchaseId))
            .ReturnsAsync((Purchase?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.NotFound");
        result.Error.Message.Should().Contain($"Purchase with ID {purchaseId} not found");

        _mockPurchaseRepository.Verify(x => x.GetByIdAsync(purchaseId), Times.Once);
        _mockPurchaseRepository.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    #endregion

    #region Forbidden Tests

    [Fact]
    public async Task Handle_WithOtherTenantPurchase_ShouldReturnForbiddenError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        var purchaseId = Guid.NewGuid();
        var command = new DeletePurchaseCommand
        {
            PurchaseId = purchaseId
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        // Purchase belongs to different tenant
        var existingPurchase = Purchase.Create(
            otherTenantId,
            "Chicken Feed",
            PurchaseType.Feed,
            250.50m,
            25m,
            QuantityUnit.Kg,
            DateTime.UtcNow.Date);

        _mockPurchaseRepository.Setup(x => x.GetByIdAsync(purchaseId))
            .ReturnsAsync(existingPurchase);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Forbidden");
        result.Error.Message.Should().Contain("do not have permission");

        _mockPurchaseRepository.Verify(x => x.GetByIdAsync(purchaseId), Times.Once);
        _mockPurchaseRepository.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    #endregion

    #region Authentication Tests

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldReturnUnauthorizedError()
    {
        // Arrange
        var purchaseId = Guid.NewGuid();
        var command = new DeletePurchaseCommand
        {
            PurchaseId = purchaseId
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Unauthorized");
        result.Error.Message.Should().Be("User is not authenticated");

        _mockPurchaseRepository.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenTenantIdNotFound_ShouldReturnUnauthorizedError()
    {
        // Arrange
        var purchaseId = Guid.NewGuid();
        var command = new DeletePurchaseCommand
        {
            PurchaseId = purchaseId
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns((Guid?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Unauthorized");
        result.Error.Message.Should().Be("Tenant not found");

        _mockPurchaseRepository.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldReturnFailureError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var purchaseId = Guid.NewGuid();
        var command = new DeletePurchaseCommand
        {
            PurchaseId = purchaseId
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        var existingPurchase = Purchase.Create(
            tenantId,
            "Chicken Feed",
            PurchaseType.Feed,
            250.50m,
            25m,
            QuantityUnit.Kg,
            DateTime.UtcNow.Date);

        _mockPurchaseRepository.Setup(x => x.GetByIdAsync(purchaseId))
            .ReturnsAsync(existingPurchase);
        _mockPurchaseRepository.Setup(x => x.DeleteAsync(purchaseId))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Failure");
        result.Error.Message.Should().Contain("Failed to delete purchase");
    }

    #endregion
}
