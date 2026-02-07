using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using Chickquita.Application.DTOs;
using Chickquita.Application.Features.Purchases.Queries;
using Chickquita.Application.Interfaces;
using Chickquita.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Chickquita.Application.Tests.Features.Purchases.Queries;

/// <summary>
/// Unit tests for GetPurchaseByIdQueryHandler.
/// Tests cover happy path, not found scenarios, and authentication.
/// </summary>
public class GetPurchaseByIdQueryHandlerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IPurchaseRepository> _mockPurchaseRepository;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<GetPurchaseByIdQueryHandler>> _mockLogger;
    private readonly GetPurchaseByIdQueryHandler _handler;

    public GetPurchaseByIdQueryHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());

        _mockPurchaseRepository = _fixture.Freeze<Mock<IPurchaseRepository>>();
        _mockCurrentUserService = _fixture.Freeze<Mock<ICurrentUserService>>();
        _mockMapper = _fixture.Freeze<Mock<IMapper>>();
        _mockLogger = _fixture.Freeze<Mock<ILogger<GetPurchaseByIdQueryHandler>>>();

        _handler = new GetPurchaseByIdQueryHandler(
            _mockPurchaseRepository.Object,
            _mockCurrentUserService.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    #region Happy Path Tests

    [Fact]
    public async Task Handle_WithValidId_ShouldReturnPurchase()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var purchaseId = Guid.NewGuid();
        var query = new GetPurchaseByIdQuery { Id = purchaseId };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        var purchase = Purchase.Create(
            tenantId,
            "Feed 1",
            PurchaseType.Feed,
            100m,
            10m,
            QuantityUnit.Kg,
            DateTime.UtcNow.Date,
            null,
            null,
            "Test notes");

        _mockPurchaseRepository.Setup(x => x.GetByIdAsync(purchaseId))
            .ReturnsAsync(purchase);

        var expectedDto = new PurchaseDto
        {
            Id = purchase.Id,
            TenantId = tenantId,
            Name = purchase.Name,
            Type = purchase.Type,
            Amount = purchase.Amount,
            Quantity = purchase.Quantity,
            Unit = purchase.Unit,
            PurchaseDate = purchase.PurchaseDate,
            Notes = purchase.Notes
        };

        _mockMapper.Setup(x => x.Map<PurchaseDto>(purchase))
            .Returns(expectedDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(purchase.Id);
        result.Value.Name.Should().Be(purchase.Name);
        result.Value.Type.Should().Be(purchase.Type);
        result.Value.Amount.Should().Be(purchase.Amount);

        _mockPurchaseRepository.Verify(x => x.GetByIdAsync(purchaseId), Times.Once);
        _mockMapper.Verify(x => x.Map<PurchaseDto>(purchase), Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidIdAndCoopReference_ShouldReturnPurchaseWithCoopId()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var purchaseId = Guid.NewGuid();
        var query = new GetPurchaseByIdQuery { Id = purchaseId };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        var purchase = Purchase.Create(
            tenantId,
            "Bedding Straw",
            PurchaseType.Bedding,
            150m,
            10m,
            QuantityUnit.Package,
            DateTime.UtcNow.Date,
            coopId);

        _mockPurchaseRepository.Setup(x => x.GetByIdAsync(purchaseId))
            .ReturnsAsync(purchase);

        var expectedDto = new PurchaseDto
        {
            Id = purchase.Id,
            TenantId = tenantId,
            CoopId = coopId,
            Name = purchase.Name,
            Type = purchase.Type,
            Amount = purchase.Amount
        };

        _mockMapper.Setup(x => x.Map<PurchaseDto>(purchase))
            .Returns(expectedDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.CoopId.Should().Be(coopId);
    }

    #endregion

    #region Not Found Tests

    [Fact]
    public async Task Handle_WithInvalidId_ShouldReturnNotFoundError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var invalidPurchaseId = Guid.NewGuid();
        var query = new GetPurchaseByIdQuery { Id = invalidPurchaseId };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        _mockPurchaseRepository.Setup(x => x.GetByIdAsync(invalidPurchaseId))
            .ReturnsAsync((Purchase?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.NotFound");
        result.Error.Message.Should().Be("Purchase not found");

        _mockPurchaseRepository.Verify(x => x.GetByIdAsync(invalidPurchaseId), Times.Once);
        _mockMapper.Verify(x => x.Map<PurchaseDto>(It.IsAny<Purchase>()), Times.Never);
    }

    #endregion

    #region Authentication Tests

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldReturnUnauthorizedError()
    {
        // Arrange
        var purchaseId = Guid.NewGuid();
        var query = new GetPurchaseByIdQuery { Id = purchaseId };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(false);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Unauthorized");
        result.Error.Message.Should().Be("User is not authenticated");

        _mockPurchaseRepository.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenTenantIdNotFound_ShouldReturnUnauthorizedError()
    {
        // Arrange
        var purchaseId = Guid.NewGuid();
        var query = new GetPurchaseByIdQuery { Id = purchaseId };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns((Guid?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Unauthorized");
        result.Error.Message.Should().Be("Tenant not found");

        _mockPurchaseRepository.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldReturnFailureError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var purchaseId = Guid.NewGuid();
        var query = new GetPurchaseByIdQuery { Id = purchaseId };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockPurchaseRepository.Setup(x => x.GetByIdAsync(purchaseId))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Failure");
        result.Error.Message.Should().Contain("Failed to retrieve purchase");
    }

    #endregion
}
