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
            Quantity = 100,
            PricePerUnit = 5.50m,
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
            TenantId = tenantId,
            Date = command.Date,
            Quantity = command.Quantity,
            PricePerUnit = command.PricePerUnit,
            BuyerName = command.BuyerName,
            Notes = command.Notes
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
        result.Value.BuyerName.Should().Be(command.BuyerName);
        result.Value.TenantId.Should().Be(tenantId);

        _mockEggSaleRepository.Verify(x => x.AddAsync(It.IsAny<EggSale>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockMapper.Verify(x => x.Map<EggSaleDto>(It.IsAny<EggSale>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNullBuyerNameAndNotes_ShouldCreateEggSaleSuccessfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var command = new CreateEggSaleCommand
        {
            Date = DateTime.UtcNow.Date,
            Quantity = 50,
            PricePerUnit = 4.00m
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        var createdEggSale = EggSale.Create(tenantId, command.Date, command.Quantity, command.PricePerUnit).Value;

        _mockEggSaleRepository.Setup(x => x.AddAsync(It.IsAny<EggSale>()))
            .ReturnsAsync(createdEggSale);

        _mockMapper.Setup(x => x.Map<EggSaleDto>(It.IsAny<EggSale>()))
            .Returns(new EggSaleDto { Id = createdEggSale.Id, TenantId = tenantId });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockEggSaleRepository.Verify(x => x.AddAsync(It.IsAny<EggSale>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithZeroPricePerUnit_ShouldCreateEggSaleSuccessfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var command = new CreateEggSaleCommand
        {
            Date = DateTime.UtcNow.Date,
            Quantity = 10,
            PricePerUnit = 0m
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        var createdEggSale = EggSale.Create(tenantId, command.Date, command.Quantity, command.PricePerUnit).Value;

        _mockEggSaleRepository.Setup(x => x.AddAsync(It.IsAny<EggSale>()))
            .ReturnsAsync(createdEggSale);

        _mockMapper.Setup(x => x.Map<EggSaleDto>(It.IsAny<EggSale>()))
            .Returns(new EggSaleDto { Id = createdEggSale.Id });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region Domain Validation Tests

    [Fact]
    public async Task Handle_WithZeroQuantity_ShouldReturnValidationError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var command = new CreateEggSaleCommand
        {
            Date = DateTime.UtcNow.Date,
            Quantity = 0,
            PricePerUnit = 5.50m
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.Validation");
        result.Error.Message.Should().Contain("Quantity must be greater than zero");

        _mockEggSaleRepository.Verify(x => x.AddAsync(It.IsAny<EggSale>()), Times.Never);
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
            PricePerUnit = 5.50m
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.Validation");

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
        result.Error.Message.Should().Contain("Price per unit cannot be negative");

        _mockEggSaleRepository.Verify(x => x.AddAsync(It.IsAny<EggSale>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithBuyerNameExceeding100Characters_ShouldReturnValidationError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var command = new CreateEggSaleCommand
        {
            Date = DateTime.UtcNow.Date,
            Quantity = 100,
            PricePerUnit = 5.50m,
            BuyerName = new string('A', 101)
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.Validation");
        result.Error.Message.Should().Contain("Buyer name cannot exceed 100 characters");

        _mockEggSaleRepository.Verify(x => x.AddAsync(It.IsAny<EggSale>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNotesExceeding500Characters_ShouldReturnValidationError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var command = new CreateEggSaleCommand
        {
            Date = DateTime.UtcNow.Date,
            Quantity = 100,
            PricePerUnit = 5.50m,
            Notes = new string('N', 501)
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.Validation");
        result.Error.Message.Should().Contain("Notes cannot exceed 500 characters");

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
            PricePerUnit = 5.50m
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

    #endregion
}
