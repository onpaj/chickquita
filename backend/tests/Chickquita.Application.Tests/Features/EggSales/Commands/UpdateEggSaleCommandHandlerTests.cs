using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using Chickquita.Application.DTOs;
using Chickquita.Application.Features.EggSales.Commands.Update;
using Chickquita.Application.Interfaces;
using Chickquita.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Chickquita.Application.Tests.Features.EggSales.Commands;

/// <summary>
/// Unit tests for UpdateEggSaleCommandHandler.
/// Tests cover happy path, not found, tenant ownership, validation errors, and exception handling.
/// </summary>
public class UpdateEggSaleCommandHandlerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IEggSaleRepository> _mockEggSaleRepository;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<UpdateEggSaleCommandHandler>> _mockLogger;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly UpdateEggSaleCommandHandler _handler;

    public UpdateEggSaleCommandHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());

        _mockEggSaleRepository = _fixture.Freeze<Mock<IEggSaleRepository>>();
        _mockCurrentUserService = _fixture.Freeze<Mock<ICurrentUserService>>();
        _mockMapper = _fixture.Freeze<Mock<IMapper>>();
        _mockLogger = _fixture.Freeze<Mock<ILogger<UpdateEggSaleCommandHandler>>>();
        _mockUnitOfWork = _fixture.Freeze<Mock<IUnitOfWork>>();
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _handler = new UpdateEggSaleCommandHandler(
            _mockEggSaleRepository.Object,
            _mockCurrentUserService.Object,
            _mockMapper.Object,
            _mockLogger.Object,
            _mockUnitOfWork.Object);
    }

    #region Happy Path Tests

    [Fact]
    public async Task Handle_WithValidData_ShouldUpdateEggSaleSuccessfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var eggSaleId = Guid.NewGuid();
        var command = new UpdateEggSaleCommand
        {
            Id = eggSaleId,
            Date = DateTime.UtcNow.Date,
            Quantity = 200,
            PricePerUnit = 6.00m,
            BuyerName = "New Buyer",
            Notes = "Updated notes"
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        var existingEggSale = EggSale.Create(tenantId, DateTime.UtcNow.Date.AddDays(-1), 100, 5.00m).Value;

        _mockEggSaleRepository.Setup(x => x.GetByIdAsync(eggSaleId))
            .ReturnsAsync(existingEggSale);

        _mockEggSaleRepository.Setup(x => x.UpdateAsync(It.IsAny<EggSale>()))
            .ReturnsAsync(existingEggSale);

        var expectedDto = new EggSaleDto
        {
            Id = existingEggSale.Id,
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

        _mockEggSaleRepository.Verify(x => x.GetByIdAsync(eggSaleId), Times.Once);
        _mockEggSaleRepository.Verify(x => x.UpdateAsync(It.IsAny<EggSale>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Not Found Tests

    [Fact]
    public async Task Handle_WhenEggSaleNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var eggSaleId = Guid.NewGuid();
        var command = new UpdateEggSaleCommand
        {
            Id = eggSaleId,
            Date = DateTime.UtcNow.Date,
            Quantity = 100,
            PricePerUnit = 5.00m
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        _mockEggSaleRepository.Setup(x => x.GetByIdAsync(eggSaleId))
            .ReturnsAsync((EggSale?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.NotFound");
        result.Error.Message.Should().Contain(eggSaleId.ToString());

        _mockEggSaleRepository.Verify(x => x.UpdateAsync(It.IsAny<EggSale>()), Times.Never);
    }

    #endregion

    #region Tenant Ownership Tests

    [Fact]
    public async Task Handle_WhenEggSaleBelongsToDifferentTenant_ShouldReturnForbiddenError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        var eggSaleId = Guid.NewGuid();
        var command = new UpdateEggSaleCommand
        {
            Id = eggSaleId,
            Date = DateTime.UtcNow.Date,
            Quantity = 100,
            PricePerUnit = 5.00m
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        // EggSale belongs to a different tenant
        var existingEggSale = EggSale.Create(otherTenantId, DateTime.UtcNow.Date, 50, 4.00m).Value;

        _mockEggSaleRepository.Setup(x => x.GetByIdAsync(eggSaleId))
            .ReturnsAsync(existingEggSale);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.Forbidden");

        _mockEggSaleRepository.Verify(x => x.UpdateAsync(It.IsAny<EggSale>()), Times.Never);
    }

    #endregion

    #region Domain Validation Tests

    [Fact]
    public async Task Handle_WithZeroQuantity_ShouldReturnValidationError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var eggSaleId = Guid.NewGuid();
        var command = new UpdateEggSaleCommand
        {
            Id = eggSaleId,
            Date = DateTime.UtcNow.Date,
            Quantity = 0,
            PricePerUnit = 5.00m
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        var existingEggSale = EggSale.Create(tenantId, DateTime.UtcNow.Date, 100, 5.00m).Value;

        _mockEggSaleRepository.Setup(x => x.GetByIdAsync(eggSaleId))
            .ReturnsAsync(existingEggSale);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.Validation");
        result.Error.Message.Should().Contain("Quantity must be greater than zero");

        _mockEggSaleRepository.Verify(x => x.UpdateAsync(It.IsAny<EggSale>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNegativePricePerUnit_ShouldReturnValidationError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var eggSaleId = Guid.NewGuid();
        var command = new UpdateEggSaleCommand
        {
            Id = eggSaleId,
            Date = DateTime.UtcNow.Date,
            Quantity = 100,
            PricePerUnit = -2.00m
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        var existingEggSale = EggSale.Create(tenantId, DateTime.UtcNow.Date, 100, 5.00m).Value;

        _mockEggSaleRepository.Setup(x => x.GetByIdAsync(eggSaleId))
            .ReturnsAsync(existingEggSale);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.Validation");
        result.Error.Message.Should().Contain("Price per unit cannot be negative");

        _mockEggSaleRepository.Verify(x => x.UpdateAsync(It.IsAny<EggSale>()), Times.Never);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldReturnFailureError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var eggSaleId = Guid.NewGuid();
        var command = new UpdateEggSaleCommand
        {
            Id = eggSaleId,
            Date = DateTime.UtcNow.Date,
            Quantity = 100,
            PricePerUnit = 5.00m
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        _mockEggSaleRepository.Setup(x => x.GetByIdAsync(eggSaleId))
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
