using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using Chickquita.Application.DTOs;
using Chickquita.Application.Features.EggSales.Queries;
using Chickquita.Application.Interfaces;
using Chickquita.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Chickquita.Application.Tests.Features.EggSales.Queries;

/// <summary>
/// Unit tests for GetEggSalesQueryHandler.
/// Tests cover happy path, date filtering, empty results, and exception handling.
/// </summary>
public class GetEggSalesQueryHandlerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IEggSaleRepository> _mockEggSaleRepository;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<GetEggSalesQueryHandler>> _mockLogger;
    private readonly GetEggSalesQueryHandler _handler;

    public GetEggSalesQueryHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());

        _mockEggSaleRepository = _fixture.Freeze<Mock<IEggSaleRepository>>();
        _mockCurrentUserService = _fixture.Freeze<Mock<ICurrentUserService>>();
        _mockMapper = _fixture.Freeze<Mock<IMapper>>();
        _mockLogger = _fixture.Freeze<Mock<ILogger<GetEggSalesQueryHandler>>>();

        _handler = new GetEggSalesQueryHandler(
            _mockEggSaleRepository.Object,
            _mockCurrentUserService.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    #region Happy Path Tests

    [Fact]
    public async Task Handle_WithNoFilters_ShouldReturnAllEggSales()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var query = new GetEggSalesQuery();

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        var eggSales = new List<EggSale>
        {
            EggSale.Create(tenantId, DateTime.UtcNow.Date, 100, 5.00m).Value,
            EggSale.Create(tenantId, DateTime.UtcNow.Date.AddDays(-1), 150, 5.50m).Value
        };

        _mockEggSaleRepository.Setup(x => x.GetWithFiltersAsync(null, null))
            .ReturnsAsync(eggSales);

        var eggSaleDtos = eggSales.Select(e => new EggSaleDto
        {
            Id = e.Id,
            TenantId = tenantId,
            Quantity = e.Quantity,
            PricePerUnit = e.PricePerUnit
        }).ToList();

        _mockMapper.Setup(x => x.Map<List<EggSaleDto>>(eggSales))
            .Returns(eggSaleDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);

        _mockEggSaleRepository.Verify(x => x.GetWithFiltersAsync(null, null), Times.Once);
    }

    [Fact]
    public async Task Handle_WithDateRangeFilter_ShouldReturnFilteredEggSales()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var fromDate = DateTime.UtcNow.Date.AddDays(-7);
        var toDate = DateTime.UtcNow.Date;
        var query = new GetEggSalesQuery
        {
            FromDate = fromDate,
            ToDate = toDate
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        var eggSales = new List<EggSale>
        {
            EggSale.Create(tenantId, DateTime.UtcNow.Date.AddDays(-3), 100, 5.00m).Value,
            EggSale.Create(tenantId, DateTime.UtcNow.Date.AddDays(-1), 120, 5.00m).Value
        };

        _mockEggSaleRepository.Setup(x => x.GetWithFiltersAsync(fromDate, toDate))
            .ReturnsAsync(eggSales);

        var eggSaleDtos = eggSales.Select(e => new EggSaleDto
        {
            Id = e.Id,
            TenantId = tenantId,
            Date = e.Date,
            Quantity = e.Quantity
        }).ToList();

        _mockMapper.Setup(x => x.Map<List<EggSaleDto>>(eggSales))
            .Returns(eggSaleDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);

        _mockEggSaleRepository.Verify(x => x.GetWithFiltersAsync(fromDate, toDate), Times.Once);
    }

    [Fact]
    public async Task Handle_WithFromDateOnly_ShouldReturnFilteredEggSales()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var fromDate = DateTime.UtcNow.Date.AddDays(-30);
        var query = new GetEggSalesQuery
        {
            FromDate = fromDate
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        var eggSales = new List<EggSale>
        {
            EggSale.Create(tenantId, DateTime.UtcNow.Date.AddDays(-20), 200, 5.00m).Value
        };

        _mockEggSaleRepository.Setup(x => x.GetWithFiltersAsync(fromDate, null))
            .ReturnsAsync(eggSales);

        _mockMapper.Setup(x => x.Map<List<EggSaleDto>>(eggSales))
            .Returns(eggSales.Select(e => new EggSaleDto { Id = e.Id, TenantId = tenantId }).ToList());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);

        _mockEggSaleRepository.Verify(x => x.GetWithFiltersAsync(fromDate, null), Times.Once);
    }

    #endregion

    #region Empty Results Tests

    [Fact]
    public async Task Handle_WithNoMatchingEggSales_ShouldReturnEmptyList()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var fromDate = DateTime.UtcNow.Date.AddDays(-7);
        var toDate = DateTime.UtcNow.Date.AddDays(-6);
        var query = new GetEggSalesQuery
        {
            FromDate = fromDate,
            ToDate = toDate
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        _mockEggSaleRepository.Setup(x => x.GetWithFiltersAsync(fromDate, toDate))
            .ReturnsAsync(new List<EggSale>());

        _mockMapper.Setup(x => x.Map<List<EggSaleDto>>(It.IsAny<List<EggSale>>()))
            .Returns(new List<EggSaleDto>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldReturnFailureError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var query = new GetEggSalesQuery();

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        _mockEggSaleRepository.Setup(x => x.GetWithFiltersAsync(null, null))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.Failure");
        result.Error.Message.Should().Be("An unexpected error occurred. Please try again.");
    }

    #endregion
}
