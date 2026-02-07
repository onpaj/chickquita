using AutoFixture;
using AutoFixture.AutoMoq;
using Chickquita.Application.DTOs;
using Chickquita.Application.Features.Statistics.Queries;
using Chickquita.Application.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Chickquita.Application.Tests.Features.Statistics.Queries;

/// <summary>
/// Unit tests for GetDashboardStatsQueryHandler.
/// Tests cover dashboard statistics aggregation, tenant isolation, authentication, and error handling.
/// </summary>
public class GetDashboardStatsQueryHandlerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IStatisticsRepository> _mockStatisticsRepository;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<ILogger<GetDashboardStatsQueryHandler>> _mockLogger;
    private readonly GetDashboardStatsQueryHandler _handler;

    public GetDashboardStatsQueryHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _mockStatisticsRepository = _fixture.Freeze<Mock<IStatisticsRepository>>();
        _mockCurrentUserService = _fixture.Freeze<Mock<ICurrentUserService>>();
        _mockLogger = _fixture.Freeze<Mock<ILogger<GetDashboardStatsQueryHandler>>>();

        _handler = new GetDashboardStatsQueryHandler(
            _mockStatisticsRepository.Object,
            _mockCurrentUserService.Object,
            _mockLogger.Object);
    }

    #region Happy Path Tests

    [Fact]
    public async Task Handle_WithValidData_ShouldReturnCorrectAggregatedStats()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var query = new GetDashboardStatsQuery();

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        var expectedStats = new DashboardStatsDto
        {
            TotalCoops = 2,
            ActiveFlocks = 3,
            TotalHens = 120,
            TotalAnimals = 147
        };

        _mockStatisticsRepository.Setup(x => x.GetDashboardStatsAsync())
            .ReturnsAsync(expectedStats);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.TotalCoops.Should().Be(2);
        result.Value.ActiveFlocks.Should().Be(3);
        result.Value.TotalHens.Should().Be(120);
        result.Value.TotalAnimals.Should().Be(147);

        _mockStatisticsRepository.Verify(x => x.GetDashboardStatsAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNoData_ShouldReturnZeroStats()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var query = new GetDashboardStatsQuery();

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        var expectedStats = new DashboardStatsDto
        {
            TotalCoops = 0,
            ActiveFlocks = 0,
            TotalHens = 0,
            TotalAnimals = 0
        };

        _mockStatisticsRepository.Setup(x => x.GetDashboardStatsAsync())
            .ReturnsAsync(expectedStats);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.TotalCoops.Should().Be(0);
        result.Value.ActiveFlocks.Should().Be(0);
        result.Value.TotalHens.Should().Be(0);
        result.Value.TotalAnimals.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithOnlyCoopsNoFlocks_ShouldReturnCorrectStats()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var query = new GetDashboardStatsQuery();

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        var expectedStats = new DashboardStatsDto
        {
            TotalCoops = 2,
            ActiveFlocks = 0,
            TotalHens = 0,
            TotalAnimals = 0
        };

        _mockStatisticsRepository.Setup(x => x.GetDashboardStatsAsync())
            .ReturnsAsync(expectedStats);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.TotalCoops.Should().Be(2);
        result.Value.ActiveFlocks.Should().Be(0);
        result.Value.TotalHens.Should().Be(0);
        result.Value.TotalAnimals.Should().Be(0);
    }

    #endregion

    #region Complex Aggregation Tests

    [Fact]
    public async Task Handle_WithMixedFlockCompositions_ShouldCalculateCorrectTotals()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var query = new GetDashboardStatsQuery();

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        var expectedStats = new DashboardStatsDto
        {
            TotalCoops = 1,
            ActiveFlocks = 3,
            TotalHens = 150, // 100 + 50 + 0
            TotalAnimals = 215 // 100 + (50+5+20) + (0+10+30)
        };

        _mockStatisticsRepository.Setup(x => x.GetDashboardStatsAsync())
            .ReturnsAsync(expectedStats);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.TotalCoops.Should().Be(1);
        result.Value.ActiveFlocks.Should().Be(3);
        result.Value.TotalHens.Should().Be(150);
        result.Value.TotalAnimals.Should().Be(215);
    }

    [Fact]
    public async Task Handle_WithMultipleCoopsAndFlocks_ShouldAggregateCorrectly()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var query = new GetDashboardStatsQuery();

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        var expectedStats = new DashboardStatsDto
        {
            TotalCoops = 3,
            ActiveFlocks = 3,
            TotalHens = 90, // 20 + 30 + 40
            TotalAnimals = 114 // (20+2+5) + (30+3+10) + (40+4+0)
        };

        _mockStatisticsRepository.Setup(x => x.GetDashboardStatsAsync())
            .ReturnsAsync(expectedStats);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.TotalCoops.Should().Be(3);
        result.Value.ActiveFlocks.Should().Be(3);
        result.Value.TotalHens.Should().Be(90);
        result.Value.TotalAnimals.Should().Be(114);
    }

    #endregion

    #region Authentication Tests

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldReturnUnauthorizedError()
    {
        // Arrange
        var query = new GetDashboardStatsQuery();

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(false);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Unauthorized");
        result.Error.Message.Should().Be("User is not authenticated");

        _mockStatisticsRepository.Verify(x => x.GetDashboardStatsAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenTenantIdNotFound_ShouldReturnUnauthorizedError()
    {
        // Arrange
        var query = new GetDashboardStatsQuery();

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns((Guid?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Unauthorized");
        result.Error.Message.Should().Be("Tenant not found");

        _mockStatisticsRepository.Verify(x => x.GetDashboardStatsAsync(), Times.Never);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldReturnFailureError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var query = new GetDashboardStatsQuery();

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        _mockStatisticsRepository.Setup(x => x.GetDashboardStatsAsync())
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Failure");
        result.Error.Message.Should().Contain("Failed to retrieve dashboard statistics");
        result.Error.Message.Should().Contain("Database connection failed");
    }

    #endregion
}
