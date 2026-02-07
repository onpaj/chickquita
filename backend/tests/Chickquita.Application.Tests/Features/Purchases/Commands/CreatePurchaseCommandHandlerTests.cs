using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using Chickquita.Application.DTOs;
using Chickquita.Application.Features.Purchases.Commands.Create;
using Chickquita.Application.Interfaces;
using Chickquita.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Chickquita.Application.Tests.Features.Purchases.Commands;

/// <summary>
/// Unit tests for CreatePurchaseCommandHandler.
/// Tests cover happy path, coop validation, and error handling.
/// </summary>
public class CreatePurchaseCommandHandlerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IPurchaseRepository> _mockPurchaseRepository;
    private readonly Mock<ICoopRepository> _mockCoopRepository;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<CreatePurchaseCommandHandler>> _mockLogger;
    private readonly CreatePurchaseCommandHandler _handler;

    public CreatePurchaseCommandHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());

        _mockPurchaseRepository = _fixture.Freeze<Mock<IPurchaseRepository>>();
        _mockCoopRepository = _fixture.Freeze<Mock<ICoopRepository>>();
        _mockCurrentUserService = _fixture.Freeze<Mock<ICurrentUserService>>();
        _mockMapper = _fixture.Freeze<Mock<IMapper>>();
        _mockLogger = _fixture.Freeze<Mock<ILogger<CreatePurchaseCommandHandler>>>();

        _handler = new CreatePurchaseCommandHandler(
            _mockPurchaseRepository.Object,
            _mockCoopRepository.Object,
            _mockCurrentUserService.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    #region Happy Path Tests

    [Fact]
    public async Task Handle_WithValidData_ShouldCreatePurchaseSuccessfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
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

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        var createdPurchase = Purchase.Create(
            tenantId,
            command.Name,
            command.Type,
            command.Amount,
            command.Quantity,
            command.Unit,
            command.PurchaseDate,
            null,
            null,
            command.Notes);

        _mockPurchaseRepository.Setup(x => x.AddAsync(It.IsAny<Purchase>()))
            .ReturnsAsync(createdPurchase);

        var expectedDto = new PurchaseDto
        {
            Id = createdPurchase.Id,
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
        result.Value.Type.Should().Be(command.Type);
        result.Value.Amount.Should().Be(command.Amount);
        result.Value.Quantity.Should().Be(command.Quantity);
        result.Value.Unit.Should().Be(command.Unit);
        result.Value.TenantId.Should().Be(tenantId);

        _mockPurchaseRepository.Verify(x => x.AddAsync(It.IsAny<Purchase>()), Times.Once);
        _mockMapper.Verify(x => x.Map<PurchaseDto>(It.IsAny<Purchase>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidCoopId_ShouldCreatePurchaseSuccessfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
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

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        var existingCoop = Coop.Create(tenantId, "Main Coop", "North Field");
        _mockCoopRepository.Setup(x => x.GetByIdAsync(coopId))
            .ReturnsAsync(existingCoop);

        var createdPurchase = Purchase.Create(
            tenantId,
            command.Name,
            command.Type,
            command.Amount,
            command.Quantity,
            command.Unit,
            command.PurchaseDate,
            coopId);

        _mockPurchaseRepository.Setup(x => x.AddAsync(It.IsAny<Purchase>()))
            .ReturnsAsync(createdPurchase);

        var expectedDto = new PurchaseDto
        {
            Id = createdPurchase.Id,
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
        _mockPurchaseRepository.Verify(x => x.AddAsync(It.IsAny<Purchase>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithConsumedDate_ShouldCreatePurchaseSuccessfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
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

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        var createdPurchase = Purchase.Create(
            tenantId,
            command.Name,
            command.Type,
            command.Amount,
            command.Quantity,
            command.Unit,
            command.PurchaseDate,
            null,
            command.ConsumedDate);

        _mockPurchaseRepository.Setup(x => x.AddAsync(It.IsAny<Purchase>()))
            .ReturnsAsync(createdPurchase);

        var expectedDto = new PurchaseDto
        {
            Id = createdPurchase.Id,
            TenantId = tenantId,
            Name = command.Name,
            Type = command.Type,
            Amount = command.Amount,
            Quantity = command.Quantity,
            Unit = command.Unit,
            PurchaseDate = command.PurchaseDate,
            ConsumedDate = command.ConsumedDate
        };

        _mockMapper.Setup(x => x.Map<PurchaseDto>(It.IsAny<Purchase>()))
            .Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.ConsumedDate.Should().Be(consumedDate);
    }

    #endregion

    #region Invalid Coop Reference Tests

    [Fact]
    public async Task Handle_WithInvalidCoopId_ShouldReturnNotFoundError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
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

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
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
        _mockPurchaseRepository.Verify(x => x.AddAsync(It.IsAny<Purchase>()), Times.Never);
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
        var command = new CreatePurchaseCommand
        {
            Name = invalidName,
            Type = PurchaseType.Feed,
            Amount = 250.50m,
            Quantity = 25m,
            Unit = QuantityUnit.Kg,
            PurchaseDate = DateTime.UtcNow.Date
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Validation");
        result.Error.Message.Should().Contain("name cannot be empty");

        _mockPurchaseRepository.Verify(x => x.AddAsync(It.IsAny<Purchase>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNegativeAmount_ShouldReturnValidationError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var command = new CreatePurchaseCommand
        {
            Name = "Chicken Feed",
            Type = PurchaseType.Feed,
            Amount = -100m,
            Quantity = 25m,
            Unit = QuantityUnit.Kg,
            PurchaseDate = DateTime.UtcNow.Date
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Validation");
        result.Error.Message.Should().Contain("cannot be negative");
    }

    [Fact]
    public async Task Handle_WithZeroQuantity_ShouldReturnValidationError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var command = new CreatePurchaseCommand
        {
            Name = "Chicken Feed",
            Type = PurchaseType.Feed,
            Amount = 250.50m,
            Quantity = 0m,
            Unit = QuantityUnit.Kg,
            PurchaseDate = DateTime.UtcNow.Date
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Validation");
        result.Error.Message.Should().Contain("must be greater than zero");
    }

    [Fact]
    public async Task Handle_WithConsumedDateBeforePurchaseDate_ShouldReturnValidationError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
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

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Validation");
        result.Error.Message.Should().Contain("before purchase date");
    }

    [Fact]
    public async Task Handle_WithNameExceeding100Characters_ShouldReturnValidationError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var longName = new string('A', 101);
        var command = new CreatePurchaseCommand
        {
            Name = longName,
            Type = PurchaseType.Feed,
            Amount = 250.50m,
            Quantity = 25m,
            Unit = QuantityUnit.Kg,
            PurchaseDate = DateTime.UtcNow.Date
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Validation");
        result.Error.Message.Should().Contain("cannot exceed 100 characters");
    }

    [Fact]
    public async Task Handle_WithNotesExceeding500Characters_ShouldReturnValidationError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var longNotes = new string('B', 501);
        var command = new CreatePurchaseCommand
        {
            Name = "Chicken Feed",
            Type = PurchaseType.Feed,
            Amount = 250.50m,
            Quantity = 25m,
            Unit = QuantityUnit.Kg,
            PurchaseDate = DateTime.UtcNow.Date,
            Notes = longNotes
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Validation");
        result.Error.Message.Should().Contain("cannot exceed 500 characters");
    }

    #endregion

    #region Authentication Tests

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldReturnUnauthorizedError()
    {
        // Arrange
        var command = new CreatePurchaseCommand
        {
            Name = "Chicken Feed",
            Type = PurchaseType.Feed,
            Amount = 250.50m,
            Quantity = 25m,
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

        _mockPurchaseRepository.Verify(x => x.AddAsync(It.IsAny<Purchase>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenTenantIdNotFound_ShouldReturnUnauthorizedError()
    {
        // Arrange
        var command = new CreatePurchaseCommand
        {
            Name = "Chicken Feed",
            Type = PurchaseType.Feed,
            Amount = 250.50m,
            Quantity = 25m,
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

        _mockPurchaseRepository.Verify(x => x.AddAsync(It.IsAny<Purchase>()), Times.Never);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldReturnFailureError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var command = new CreatePurchaseCommand
        {
            Name = "Chicken Feed",
            Type = PurchaseType.Feed,
            Amount = 250.50m,
            Quantity = 25m,
            Unit = QuantityUnit.Kg,
            PurchaseDate = DateTime.UtcNow.Date
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockPurchaseRepository.Setup(x => x.AddAsync(It.IsAny<Purchase>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Failure");
        result.Error.Message.Should().Contain("Failed to create purchase");
    }

    #endregion
}
