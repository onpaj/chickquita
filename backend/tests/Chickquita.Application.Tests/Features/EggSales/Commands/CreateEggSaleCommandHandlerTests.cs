using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using Chickquita.Application.DTOs;
using Chickquita.Application.Features.EggSales.Commands.Create;
using Chickquita.Application.Interfaces;
using Chickquita.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Chickquita.Application.Tests.Features.EggSales.Commands;

/// <summary>
/// Unit tests for CreateEggSaleCommandHandler.
/// Tests cover happy path, domain validation errors, and exception handling.
/// </summary>
public class CreateEggSaleCommandHandlerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IEggSaleRepository> _mockEggSaleRepository;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<CreateEggSaleCommandHandler>> _mockLogger;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly CreateEggSaleCommandHandler _handler;

    public CreateEggSaleCommandHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());

        _mockEggSaleRepository = _fixture.Freeze<Mock<IEggSaleRepository>>();
        _mockCurrentUserService = _fixture.Freeze<Mock<ICurrentUserService>>();
        _mockMapper = _fixture.Freeze<Mock<IMapper>>();
        _mockLogger = _fixture.Freeze<Mock<ILogger<CreateEggSaleCommandHandler>>>();
        _mockUnitOfWork = _fixture.Freeze<Mock<IUnitOfWork>>();
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _handler = new CreateEggSaleCommandHandler(
            _mockEggSaleRepository.Object,
            _mockCurrentUserService.Object,
            _mockMapper.Object,
            _mockLogger.Object,
            _mockUnitOfWork.Object);
    }

    #region Happy Path Tests

    [Fact]
    public async Task Handle_WithValidData_ShouldCreateEggSaleSuccessfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var command = new CreateEggSaleCommand
        {
            Date = DateTime.UtcNow.Date,
            Quantity = 120,
            PricePerUnit = 4.50m,
            BuyerName = "Local Market",
            Notes = "Weekly batch"
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        var createdEggSale = EggSale.Create(
            tenantId,
            command.Date,
            command.Quantity,
            command.PricePerUnit,
            command.BuyerName,
            command.Notes).Value;

        _mockEggSaleRepository.Setup(x => x.AddAsync(It.IsAny<EggSale>()))
            .ReturnsAsync(createdEggSale);

        var expectedDto = new EggSaleDto
        {
            Id = createdEggSale.Id,
            Date = createdEggSale.Date,
            Quantity = command.Quantity,
            PricePerUnit = command.PricePerUnit,
            TotalRevenue = command.Quantity * command.PricePerUnit,
            BuyerName = command.BuyerName,
            Notes = command.Notes,
            CreatedAt = createdEggSale.CreatedAt
        };

        _mockMapper.Setup(x => x.Map<EggSaleDto>(It.IsAny<EggSale>()))
            .Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Quantity.Should().Be(command.Quantity);
        result.Value.PricePerUnit.Should().Be(command.PricePerUnit);
        result.Value.TotalRevenue.Should().Be(command.Quantity * command.PricePerUnit);
        result.Value.BuyerName.Should().Be(command.BuyerName);
        result.Value.Notes.Should().Be(command.Notes);

        _mockEggSaleRepository.Verify(x => x.AddAsync(It.IsAny<EggSale>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockMapper.Verify(x => x.Map<EggSaleDto>(It.IsAny<EggSale>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithMinimalData_ShouldCreateEggSaleSuccessfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var command = new CreateEggSaleCommand
        {
            Date = DateTime.UtcNow.Date,
            Quantity = 10,
            PricePerUnit = 3.00m
            // BuyerName and Notes are optional — omitted
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        var createdEggSale = EggSale.Create(
            tenantId,
            command.Date,
            command.Quantity,
            command.PricePerUnit).Value;

        _mockEggSaleRepository.Setup(x => x.AddAsync(It.IsAny<EggSale>()))
            .ReturnsAsync(createdEggSale);

        var expectedDto = new EggSaleDto
        {
            Id = createdEggSale.Id,
            Date = createdEggSale.Date,
            Quantity = command.Quantity,
            PricePerUnit = command.PricePerUnit,
            TotalRevenue = command.Quantity * command.PricePerUnit
        };

        _mockMapper.Setup(x => x.Map<EggSaleDto>(It.IsAny<EggSale>()))
            .Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.BuyerName.Should().BeNull();
        result.Value.Notes.Should().BeNull();

        _mockEggSaleRepository.Verify(x => x.AddAsync(It.IsAny<EggSale>()), Times.Once);
    }

    #endregion

    #region Domain Validation Error Tests

    [Fact]
    public async Task Handle_WithZeroQuantity_ShouldReturnValidationError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var command = new CreateEggSaleCommand
        {
            Date = DateTime.UtcNow.Date,
            Quantity = 0,
            PricePerUnit = 4.50m
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.Validation");
        result.Error.Message.Should().Be("Quantity must be greater than zero.");

        _mockEggSaleRepository.Verify(x => x.AddAsync(It.IsAny<EggSale>()), Times.Never);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNegativeQuantity_ShouldReturnValidationError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var command = new CreateEggSaleCommand
        {
            Date = DateTime.UtcNow.Date,
            Quantity = -5,
            PricePerUnit = 4.50m
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.Validation");
        result.Error.Message.Should().Be("Quantity must be greater than zero.");

        _mockEggSaleRepository.Verify(x => x.AddAsync(It.IsAny<EggSale>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithZeroPricePerUnit_ShouldReturnValidationError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var command = new CreateEggSaleCommand
        {
            Date = DateTime.UtcNow.Date,
            Quantity = 100,
            PricePerUnit = 0m
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.Validation");
        result.Error.Message.Should().Be("Price per unit must be greater than zero.");

        _mockEggSaleRepository.Verify(x => x.AddAsync(It.IsAny<EggSale>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNegativePricePerUnit_ShouldReturnValidationError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var command = new CreateEggSaleCommand
        {
            Date = DateTime.UtcNow.Date,
            Quantity = 100,
            PricePerUnit = -1.00m
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.Validation");
        result.Error.Message.Should().Be("Price per unit must be greater than zero.");

        _mockEggSaleRepository.Verify(x => x.AddAsync(It.IsAny<EggSale>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithBuyerNameExceeding200Characters_ShouldReturnValidationError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var command = new CreateEggSaleCommand
        {
            Date = DateTime.UtcNow.Date,
            Quantity = 100,
            PricePerUnit = 4.50m,
            BuyerName = new string('X', 201)
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.Validation");
        result.Error.Message.Should().Be("Buyer name cannot exceed 200 characters.");

        _mockEggSaleRepository.Verify(x => x.AddAsync(It.IsAny<EggSale>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNotesExceeding1000Characters_ShouldReturnValidationError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var command = new CreateEggSaleCommand
        {
            Date = DateTime.UtcNow.Date,
            Quantity = 100,
            PricePerUnit = 4.50m,
            Notes = new string('N', 1001)
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.Validation");
        result.Error.Message.Should().Be("Notes cannot exceed 1000 characters.");

        _mockEggSaleRepository.Verify(x => x.AddAsync(It.IsAny<EggSale>()), Times.Never);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldReturnFailureError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var command = new CreateEggSaleCommand
        {
            Date = DateTime.UtcNow.Date,
            Quantity = 100,
            PricePerUnit = 4.50m
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockEggSaleRepository.Setup(x => x.AddAsync(It.IsAny<EggSale>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.Failure");
        result.Error.Message.Should().Be("An unexpected error occurred. Please try again.");
    }

    [Fact]
    public async Task Handle_WhenUnitOfWorkThrowsException_ShouldReturnFailureError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var command = new CreateEggSaleCommand
        {
            Date = DateTime.UtcNow.Date,
            Quantity = 50,
            PricePerUnit = 5.00m
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        var createdEggSale = EggSale.Create(tenantId, command.Date, command.Quantity, command.PricePerUnit).Value;
        _mockEggSaleRepository.Setup(x => x.AddAsync(It.IsAny<EggSale>()))
            .ReturnsAsync(createdEggSale);

        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Transaction failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.Failure");
        result.Error.Message.Should().Be("An unexpected error occurred. Please try again.");
    }

    #endregion
}
