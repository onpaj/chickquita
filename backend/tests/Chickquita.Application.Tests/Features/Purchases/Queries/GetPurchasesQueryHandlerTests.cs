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
/// Unit tests for GetPurchasesQueryHandler.
/// Tests cover filtering, empty results, and tenant isolation.
/// </summary>
public class GetPurchasesQueryHandlerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IPurchaseRepository> _mockPurchaseRepository;
    private readonly Mock<IFlockRepository> _mockFlockRepository;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<GetPurchasesQueryHandler>> _mockLogger;
    private readonly GetPurchasesQueryHandler _handler;

    public GetPurchasesQueryHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());

        _mockPurchaseRepository = _fixture.Freeze<Mock<IPurchaseRepository>>();
        _mockFlockRepository = _fixture.Freeze<Mock<IFlockRepository>>();
        _mockCurrentUserService = _fixture.Freeze<Mock<ICurrentUserService>>();
        _mockMapper = _fixture.Freeze<Mock<IMapper>>();
        _mockLogger = _fixture.Freeze<Mock<ILogger<GetPurchasesQueryHandler>>>();

        _handler = new GetPurchasesQueryHandler(
            _mockPurchaseRepository.Object,
            _mockFlockRepository.Object,
            _mockCurrentUserService.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    #region Happy Path Tests

    [Fact]
    public async Task Handle_WithNoFilters_ShouldReturnAllPurchases()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var query = new GetPurchasesQuery();

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        var purchases = new List<Purchase>
        {
            Purchase.Create(tenantId, "Feed 1", PurchaseType.Feed, 100m, 10m, QuantityUnit.Kg, DateTime.UtcNow.Date),
            Purchase.Create(tenantId, "Bedding 1", PurchaseType.Bedding, 50m, 5m, QuantityUnit.Package, DateTime.UtcNow.Date.AddDays(-1))
        };

        _mockPurchaseRepository.Setup(x => x.GetWithFiltersAsync(null, null, null, null))
            .ReturnsAsync(purchases);

        var purchaseDtos = purchases.Select(p => new PurchaseDto
        {
            Id = p.Id,
            TenantId = tenantId,
            Name = p.Name,
            Type = p.Type,
            Amount = p.Amount
        }).ToList();

        _mockMapper.Setup(x => x.Map<List<PurchaseDto>>(purchases))
            .Returns(purchaseDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);

        _mockPurchaseRepository.Verify(x => x.GetWithFiltersAsync(null, null, null, null), Times.Once);
    }

    [Fact]
    public async Task Handle_WithDateRangeFilter_ShouldReturnFilteredPurchases()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var fromDate = DateTime.UtcNow.Date.AddDays(-7);
        var toDate = DateTime.UtcNow.Date;
        var query = new GetPurchasesQuery
        {
            FromDate = fromDate,
            ToDate = toDate
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        var purchases = new List<Purchase>
        {
            Purchase.Create(tenantId, "Feed 1", PurchaseType.Feed, 100m, 10m, QuantityUnit.Kg, DateTime.UtcNow.Date.AddDays(-3)),
            Purchase.Create(tenantId, "Feed 2", PurchaseType.Feed, 120m, 12m, QuantityUnit.Kg, DateTime.UtcNow.Date.AddDays(-1))
        };

        _mockPurchaseRepository.Setup(x => x.GetWithFiltersAsync(fromDate, toDate, null, null))
            .ReturnsAsync(purchases);

        var purchaseDtos = purchases.Select(p => new PurchaseDto
        {
            Id = p.Id,
            TenantId = tenantId,
            Name = p.Name,
            PurchaseDate = p.PurchaseDate
        }).ToList();

        _mockMapper.Setup(x => x.Map<List<PurchaseDto>>(purchases))
            .Returns(purchaseDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.All(p => p.PurchaseDate >= fromDate && p.PurchaseDate <= toDate).Should().BeTrue();

        _mockPurchaseRepository.Verify(x => x.GetWithFiltersAsync(fromDate, toDate, null, null), Times.Once);
    }

    [Fact]
    public async Task Handle_WithTypeFilter_ShouldReturnFilteredPurchases()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var query = new GetPurchasesQuery
        {
            Type = PurchaseType.Feed
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        var purchases = new List<Purchase>
        {
            Purchase.Create(tenantId, "Feed 1", PurchaseType.Feed, 100m, 10m, QuantityUnit.Kg, DateTime.UtcNow.Date),
            Purchase.Create(tenantId, "Feed 2", PurchaseType.Feed, 120m, 12m, QuantityUnit.Kg, DateTime.UtcNow.Date.AddDays(-1))
        };

        _mockPurchaseRepository.Setup(x => x.GetWithFiltersAsync(null, null, PurchaseType.Feed, null))
            .ReturnsAsync(purchases);

        var purchaseDtos = purchases.Select(p => new PurchaseDto
        {
            Id = p.Id,
            TenantId = tenantId,
            Name = p.Name,
            Type = p.Type
        }).ToList();

        _mockMapper.Setup(x => x.Map<List<PurchaseDto>>(purchases))
            .Returns(purchaseDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.All(p => p.Type == PurchaseType.Feed).Should().BeTrue();

        _mockPurchaseRepository.Verify(x => x.GetWithFiltersAsync(null, null, PurchaseType.Feed, null), Times.Once);
    }

    [Fact]
    public async Task Handle_WithFlockIdFilter_ShouldResolveCoopAndFilterPurchases()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var flockId = Guid.NewGuid();
        var query = new GetPurchasesQuery
        {
            FlockId = flockId
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        var flock = Flock.Create(tenantId, coopId, "Test Flock", DateTime.UtcNow.Date.AddMonths(-2), 10, 2, 0);
        _mockFlockRepository.Setup(x => x.GetByIdAsync(flockId))
            .ReturnsAsync(flock);

        var purchases = new List<Purchase>
        {
            Purchase.Create(tenantId, "Feed 1", PurchaseType.Feed, 100m, 10m, QuantityUnit.Kg, DateTime.UtcNow.Date, coopId)
        };

        _mockPurchaseRepository.Setup(x => x.GetWithFiltersAsync(null, null, null, coopId))
            .ReturnsAsync(purchases);

        var purchaseDtos = purchases.Select(p => new PurchaseDto
        {
            Id = p.Id,
            TenantId = tenantId,
            CoopId = coopId,
            Name = p.Name
        }).ToList();

        _mockMapper.Setup(x => x.Map<List<PurchaseDto>>(purchases))
            .Returns(purchaseDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.All(p => p.CoopId == coopId).Should().BeTrue();

        _mockFlockRepository.Verify(x => x.GetByIdAsync(flockId), Times.Once);
        _mockPurchaseRepository.Verify(x => x.GetWithFiltersAsync(null, null, null, coopId), Times.Once);
    }

    [Fact]
    public async Task Handle_WithAllFilters_ShouldReturnFilteredPurchases()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var flockId = Guid.NewGuid();
        var fromDate = DateTime.UtcNow.Date.AddDays(-7);
        var toDate = DateTime.UtcNow.Date;
        var query = new GetPurchasesQuery
        {
            FromDate = fromDate,
            ToDate = toDate,
            Type = PurchaseType.Feed,
            FlockId = flockId
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        var flock = Flock.Create(tenantId, coopId, "Test Flock", DateTime.UtcNow.Date.AddMonths(-2), 10, 2, 0);
        _mockFlockRepository.Setup(x => x.GetByIdAsync(flockId))
            .ReturnsAsync(flock);

        var purchases = new List<Purchase>
        {
            Purchase.Create(tenantId, "Feed 1", PurchaseType.Feed, 100m, 10m, QuantityUnit.Kg, DateTime.UtcNow.Date.AddDays(-2), coopId)
        };

        _mockPurchaseRepository.Setup(x => x.GetWithFiltersAsync(fromDate, toDate, PurchaseType.Feed, coopId))
            .ReturnsAsync(purchases);

        var purchaseDtos = purchases.Select(p => new PurchaseDto
        {
            Id = p.Id,
            TenantId = tenantId,
            CoopId = coopId,
            Name = p.Name,
            Type = p.Type,
            PurchaseDate = p.PurchaseDate
        }).ToList();

        _mockMapper.Setup(x => x.Map<List<PurchaseDto>>(purchases))
            .Returns(purchaseDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);

        _mockFlockRepository.Verify(x => x.GetByIdAsync(flockId), Times.Once);
        _mockPurchaseRepository.Verify(x => x.GetWithFiltersAsync(fromDate, toDate, PurchaseType.Feed, coopId), Times.Once);
    }

    #endregion

    #region Empty Results Tests

    [Fact]
    public async Task Handle_WithNoMatchingPurchases_ShouldReturnEmptyList()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var query = new GetPurchasesQuery
        {
            Type = PurchaseType.Veterinary
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        _mockPurchaseRepository.Setup(x => x.GetWithFiltersAsync(null, null, PurchaseType.Veterinary, null))
            .ReturnsAsync(new List<Purchase>());

        _mockMapper.Setup(x => x.Map<List<PurchaseDto>>(It.IsAny<List<Purchase>>()))
            .Returns(new List<PurchaseDto>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }

    #endregion

    #region Flock Not Found Tests

    [Fact]
    public async Task Handle_WithInvalidFlockId_ShouldReturnNotFoundError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var invalidFlockId = Guid.NewGuid();
        var query = new GetPurchasesQuery
        {
            FlockId = invalidFlockId
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        _mockFlockRepository.Setup(x => x.GetByIdAsync(invalidFlockId))
            .ReturnsAsync((Flock?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.NotFound");
        result.Error.Message.Should().Contain($"Flock with ID {invalidFlockId} not found");

        _mockFlockRepository.Verify(x => x.GetByIdAsync(invalidFlockId), Times.Once);
        _mockPurchaseRepository.Verify(x => x.GetWithFiltersAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<PurchaseType?>(), It.IsAny<Guid?>()), Times.Never);
    }

    #endregion

    #region Authentication Tests

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldReturnUnauthorizedError()
    {
        // Arrange
        var query = new GetPurchasesQuery();

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(false);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Unauthorized");
        result.Error.Message.Should().Be("User is not authenticated");

        _mockPurchaseRepository.Verify(x => x.GetWithFiltersAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<PurchaseType?>(), It.IsAny<Guid?>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenTenantIdNotFound_ShouldReturnUnauthorizedError()
    {
        // Arrange
        var query = new GetPurchasesQuery();

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns((Guid?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Unauthorized");
        result.Error.Message.Should().Be("Tenant not found");

        _mockPurchaseRepository.Verify(x => x.GetWithFiltersAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<PurchaseType?>(), It.IsAny<Guid?>()), Times.Never);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldReturnFailureError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var query = new GetPurchasesQuery();

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockPurchaseRepository.Setup(x => x.GetWithFiltersAsync(null, null, null, null))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Failure");
        result.Error.Message.Should().Contain("Failed to retrieve purchases");
    }

    #endregion
}
