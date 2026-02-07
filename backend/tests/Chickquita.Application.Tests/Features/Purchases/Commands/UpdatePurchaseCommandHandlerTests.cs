using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using Chickquita.Application.DTOs;
using Chickquita.Application.Features.Purchases.Commands.Update;
using Chickquita.Application.Interfaces;
using Chickquita.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Chickquita.Application.Tests.Features.Purchases.Commands;

/// <summary>
/// Unit tests for UpdatePurchaseCommandHandler.
/// Tests cover happy path, validation, tenant ownership, and error handling.
/// </summary>
public class UpdatePurchaseCommandHandlerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IPurchaseRepository> _mockPurchaseRepository;
    private readonly Mock<ICoopRepository> _mockCoopRepository;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<UpdatePurchaseCommandHandler>> _mockLogger;
    private readonly UpdatePurchaseCommandHandler _handler;

    public UpdatePurchaseCommandHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());

        _mockPurchaseRepository = _fixture.Freeze<Mock<IPurchaseRepository>>();
        _mockCoopRepository = _fixture.Freeze<Mock<ICoopRepository>>();
        _mockCurrentUserService = _fixture.Freeze<Mock<ICurrentUserService>>();
        _mockMapper = _fixture.Freeze<Mock<IMapper>>();
        _mockLogger = _fixture.Freeze<Mock<ILogger<UpdatePurchaseCommandHandler>>>();

        _handler = new UpdatePurchaseCommandHandler(
            _mockPurchaseRepository.Object,
            _mockCoopRepository.Object,
            _mockCurrentUserService.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    #region Happy Path Tests

    [Fact]
    public async Task Handle_WithValidData_ShouldUpdatePurchaseSuccessfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var purchaseId = Guid.NewGuid();
        var command = new UpdatePurchaseCommand
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

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        var existingPurchase = Purchase.Create(
            tenantId,
            "Old Feed",
            PurchaseType.Feed,
            250.50m,
            25m,
            QuantityUnit.Kg,
            DateTime.UtcNow.Date.AddDays(-1));

        _mockPurchaseRepository.Setup(x => x.GetByIdAsync(purchaseId))
            .ReturnsAsync(existingPurchase);

        _mockPurchaseRepository.Setup(x => x.UpdateAsync(It.IsAny<Purchase>()))
            .ReturnsAsync(existingPurchase);

        var expectedDto = new PurchaseDto
        {
            Id = purchaseId,
            TenantId = tenantId,
            Name = command.Name,
            Type = command.Type,
            Amount = command.Amount,
            Quantity = command.Quantity,
            Unit = command.Unit,
            PurchaseDate = command.PurchaseDate,
            Notes = command.Notes
        };

        _mockMapper.Setup(x => x.Map<PurchaseDto>(It.IsAny<Purchase>()))
            .Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be(command.Name);
        result.Value.Amount.Should().Be(command.Amount);

        _mockPurchaseRepository.Verify(x => x.GetByIdAsync(purchaseId), Times.Once);
        _mockPurchaseRepository.Verify(x => x.UpdateAsync(It.IsAny<Purchase>()), Times.Once);
        _mockMapper.Verify(x => x.Map<PurchaseDto>(It.IsAny<Purchase>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidCoopId_ShouldUpdatePurchaseSuccessfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var purchaseId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var command = new UpdatePurchaseCommand
        {
            Id = purchaseId,
            CoopId = coopId,
            Name = "Updated Bedding",
            Type = PurchaseType.Bedding,
            Amount = 200.00m,
            Quantity = 15m,
            Unit = QuantityUnit.Package,
            PurchaseDate = DateTime.UtcNow.Date
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        var existingPurchase = Purchase.Create(
            tenantId,
            "Old Bedding",
            PurchaseType.Bedding,
            150.00m,
            10m,
            QuantityUnit.Package,
            DateTime.UtcNow.Date.AddDays(-1));

        _mockPurchaseRepository.Setup(x => x.GetByIdAsync(purchaseId))
            .ReturnsAsync(existingPurchase);

        var existingCoop = Coop.Create(tenantId, "Main Coop", "North Field");
        _mockCoopRepository.Setup(x => x.GetByIdAsync(coopId))
            .ReturnsAsync(existingCoop);

        _mockPurchaseRepository.Setup(x => x.UpdateAsync(It.IsAny<Purchase>()))
            .ReturnsAsync(existingPurchase);

        var expectedDto = new PurchaseDto
        {
            Id = purchaseId,
            TenantId = tenantId,
            CoopId = coopId,
            Name = command.Name,
            Type = command.Type,
            Amount = command.Amount,
            Quantity = command.Quantity,
            Unit = command.Unit,
            PurchaseDate = command.PurchaseDate
        };

        _mockMapper.Setup(x => x.Map<PurchaseDto>(It.IsAny<Purchase>()))
            .Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.CoopId.Should().Be(coopId);

        _mockCoopRepository.Verify(x => x.GetByIdAsync(coopId), Times.Once);
        _mockPurchaseRepository.Verify(x => x.UpdateAsync(It.IsAny<Purchase>()), Times.Once);
    }

    #endregion

    #region Not Found Tests

    [Fact]
    public async Task Handle_WithNonExistentPurchase_ShouldReturnNotFoundError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var purchaseId = Guid.NewGuid();
        var command = new UpdatePurchaseCommand
        {
            Id = purchaseId,
            Name = "Updated Feed",
            Type = PurchaseType.Feed,
            Amount = 300.00m,
            Quantity = 30m,
            Unit = QuantityUnit.Kg,
            PurchaseDate = DateTime.UtcNow.Date
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
        _mockPurchaseRepository.Verify(x => x.UpdateAsync(It.IsAny<Purchase>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithInvalidCoopId_ShouldReturnNotFoundError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var purchaseId = Guid.NewGuid();
        var invalidCoopId = Guid.NewGuid();
        var command = new UpdatePurchaseCommand
        {
            Id = purchaseId,
            CoopId = invalidCoopId,
            Name = "Updated Bedding",
            Type = PurchaseType.Bedding,
            Amount = 200.00m,
            Quantity = 15m,
            Unit = QuantityUnit.Package,
            PurchaseDate = DateTime.UtcNow.Date
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        var existingPurchase = Purchase.Create(
            tenantId,
            "Old Bedding",
            PurchaseType.Bedding,
            150.00m,
            10m,
            QuantityUnit.Package,
            DateTime.UtcNow.Date.AddDays(-1));

        _mockPurchaseRepository.Setup(x => x.GetByIdAsync(purchaseId))
            .ReturnsAsync(existingPurchase);
        _mockCoopRepository.Setup(x => x.GetByIdAsync(invalidCoopId))
            .ReturnsAsync((Coop?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.NotFound");
        result.Error.Message.Should().Contain($"Coop with ID {invalidCoopId} not found");

        _mockCoopRepository.Verify(x => x.GetByIdAsync(invalidCoopId), Times.Once);
        _mockPurchaseRepository.Verify(x => x.UpdateAsync(It.IsAny<Purchase>()), Times.Never);
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
        var command = new UpdatePurchaseCommand
        {
            Id = purchaseId,
            Name = "Updated Feed",
            Type = PurchaseType.Feed,
            Amount = 300.00m,
            Quantity = 30m,
            Unit = QuantityUnit.Kg,
            PurchaseDate = DateTime.UtcNow.Date
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        // Purchase belongs to different tenant
        var existingPurchase = Purchase.Create(
            otherTenantId,
            "Old Feed",
            PurchaseType.Feed,
            250.50m,
            25m,
            QuantityUnit.Kg,
            DateTime.UtcNow.Date.AddDays(-1));

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
        _mockPurchaseRepository.Verify(x => x.UpdateAsync(It.IsAny<Purchase>()), Times.Never);
    }

    #endregion

    #region Validation Tests

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_WithEmptyOrWhitespaceName_ShouldReturnValidationError(string invalidName)
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var purchaseId = Guid.NewGuid();
        var command = new UpdatePurchaseCommand
        {
            Id = purchaseId,
            Name = invalidName,
            Type = PurchaseType.Feed,
            Amount = 300.00m,
            Quantity = 30m,
            Unit = QuantityUnit.Kg,
            PurchaseDate = DateTime.UtcNow.Date
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        var existingPurchase = Purchase.Create(
            tenantId,
            "Old Feed",
            PurchaseType.Feed,
            250.50m,
            25m,
            QuantityUnit.Kg,
            DateTime.UtcNow.Date.AddDays(-1));

        _mockPurchaseRepository.Setup(x => x.GetByIdAsync(purchaseId))
            .ReturnsAsync(existingPurchase);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Validation");
        result.Error.Message.Should().Contain("name cannot be empty");

        _mockPurchaseRepository.Verify(x => x.UpdateAsync(It.IsAny<Purchase>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNegativeAmount_ShouldReturnValidationError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var purchaseId = Guid.NewGuid();
        var command = new UpdatePurchaseCommand
        {
            Id = purchaseId,
            Name = "Updated Feed",
            Type = PurchaseType.Feed,
            Amount = -100m,
            Quantity = 30m,
            Unit = QuantityUnit.Kg,
            PurchaseDate = DateTime.UtcNow.Date
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        var existingPurchase = Purchase.Create(
            tenantId,
            "Old Feed",
            PurchaseType.Feed,
            250.50m,
            25m,
            QuantityUnit.Kg,
            DateTime.UtcNow.Date.AddDays(-1));

        _mockPurchaseRepository.Setup(x => x.GetByIdAsync(purchaseId))
            .ReturnsAsync(existingPurchase);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Validation");
        result.Error.Message.Should().Contain("cannot be negative");
    }

    #endregion

    #region Authentication Tests

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldReturnUnauthorizedError()
    {
        // Arrange
        var purchaseId = Guid.NewGuid();
        var command = new UpdatePurchaseCommand
        {
            Id = purchaseId,
            Name = "Updated Feed",
            Type = PurchaseType.Feed,
            Amount = 300.00m,
            Quantity = 30m,
            Unit = QuantityUnit.Kg,
            PurchaseDate = DateTime.UtcNow.Date
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Unauthorized");
        result.Error.Message.Should().Be("User is not authenticated");

        _mockPurchaseRepository.Verify(x => x.UpdateAsync(It.IsAny<Purchase>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenTenantIdNotFound_ShouldReturnUnauthorizedError()
    {
        // Arrange
        var purchaseId = Guid.NewGuid();
        var command = new UpdatePurchaseCommand
        {
            Id = purchaseId,
            Name = "Updated Feed",
            Type = PurchaseType.Feed,
            Amount = 300.00m,
            Quantity = 30m,
            Unit = QuantityUnit.Kg,
            PurchaseDate = DateTime.UtcNow.Date
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

        _mockPurchaseRepository.Verify(x => x.UpdateAsync(It.IsAny<Purchase>()), Times.Never);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldReturnFailureError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var purchaseId = Guid.NewGuid();
        var command = new UpdatePurchaseCommand
        {
            Id = purchaseId,
            Name = "Updated Feed",
            Type = PurchaseType.Feed,
            Amount = 300.00m,
            Quantity = 30m,
            Unit = QuantityUnit.Kg,
            PurchaseDate = DateTime.UtcNow.Date
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        var existingPurchase = Purchase.Create(
            tenantId,
            "Old Feed",
            PurchaseType.Feed,
            250.50m,
            25m,
            QuantityUnit.Kg,
            DateTime.UtcNow.Date.AddDays(-1));

        _mockPurchaseRepository.Setup(x => x.GetByIdAsync(purchaseId))
            .ReturnsAsync(existingPurchase);
        _mockPurchaseRepository.Setup(x => x.UpdateAsync(It.IsAny<Purchase>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Failure");
        result.Error.Message.Should().Contain("Failed to update purchase");
    }

    #endregion
}
