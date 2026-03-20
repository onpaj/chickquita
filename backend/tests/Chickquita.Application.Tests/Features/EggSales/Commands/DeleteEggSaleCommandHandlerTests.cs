using AutoFixture;
using AutoFixture.AutoMoq;
using Chickquita.Application.Features.EggSales.Commands.Delete;
using Chickquita.Application.Interfaces;
using Chickquita.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Chickquita.Application.Tests.Features.EggSales.Commands;

/// <summary>
/// Unit tests for DeleteEggSaleCommandHandler.
/// Tests cover happy path, not found, tenant ownership, and exception handling.
/// </summary>
public class DeleteEggSaleCommandHandlerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IEggSaleRepository> _mockEggSaleRepository;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<ILogger<DeleteEggSaleCommandHandler>> _mockLogger;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly DeleteEggSaleCommandHandler _handler;

    public DeleteEggSaleCommandHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());

        _mockEggSaleRepository = _fixture.Freeze<Mock<IEggSaleRepository>>();
        _mockCurrentUserService = _fixture.Freeze<Mock<ICurrentUserService>>();
        _mockLogger = _fixture.Freeze<Mock<ILogger<DeleteEggSaleCommandHandler>>>();
        _mockUnitOfWork = _fixture.Freeze<Mock<IUnitOfWork>>();
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _handler = new DeleteEggSaleCommandHandler(
            _mockEggSaleRepository.Object,
            _mockCurrentUserService.Object,
            _mockLogger.Object,
            _mockUnitOfWork.Object);
    }

    #region Happy Path Tests

    [Fact]
    public async Task Handle_WithValidEggSaleId_ShouldDeleteSuccessfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var eggSaleId = Guid.NewGuid();
        var command = new DeleteEggSaleCommand { EggSaleId = eggSaleId };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        var existingEggSale = EggSale.Create(tenantId, DateTime.UtcNow.Date, 100, 5.00m).Value;

        _mockEggSaleRepository.Setup(x => x.GetByIdAsync(eggSaleId))
            .ReturnsAsync(existingEggSale);

        _mockEggSaleRepository.Setup(x => x.DeleteAsync(eggSaleId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();

        _mockEggSaleRepository.Verify(x => x.GetByIdAsync(eggSaleId), Times.Once);
        _mockEggSaleRepository.Verify(x => x.DeleteAsync(eggSaleId), Times.Once);
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
        var command = new DeleteEggSaleCommand { EggSaleId = eggSaleId };

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

        _mockEggSaleRepository.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
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
        var command = new DeleteEggSaleCommand { EggSaleId = eggSaleId };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        // EggSale belongs to a different tenant
        var existingEggSale = EggSale.Create(otherTenantId, DateTime.UtcNow.Date, 100, 5.00m).Value;

        _mockEggSaleRepository.Setup(x => x.GetByIdAsync(eggSaleId))
            .ReturnsAsync(existingEggSale);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.Forbidden");

        _mockEggSaleRepository.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldReturnFailureError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var eggSaleId = Guid.NewGuid();
        var command = new DeleteEggSaleCommand { EggSaleId = eggSaleId };

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
