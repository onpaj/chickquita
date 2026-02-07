using AutoFixture;
using AutoFixture.AutoMoq;
using Chickquita.Application.Features.Purchases.Queries;
using Chickquita.Application.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Chickquita.Application.Tests.Features.Purchases.Queries;

/// <summary>
/// Unit tests for GetPurchaseNamesQueryHandler.
/// Tests cover autocomplete functionality, empty queries, and limits.
/// </summary>
public class GetPurchaseNamesQueryHandlerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IPurchaseRepository> _mockPurchaseRepository;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<ILogger<GetPurchaseNamesQueryHandler>> _mockLogger;
    private readonly GetPurchaseNamesQueryHandler _handler;

    public GetPurchaseNamesQueryHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());

        _mockPurchaseRepository = _fixture.Freeze<Mock<IPurchaseRepository>>();
        _mockCurrentUserService = _fixture.Freeze<Mock<ICurrentUserService>>();
        _mockLogger = _fixture.Freeze<Mock<ILogger<GetPurchaseNamesQueryHandler>>>();

        _handler = new GetPurchaseNamesQueryHandler(
            _mockPurchaseRepository.Object,
            _mockCurrentUserService.Object,
            _mockLogger.Object);
    }

    #region Happy Path Tests

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnMatchingNames()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var query = new GetPurchaseNamesQuery
        {
            Query = "krm",
            Limit = 20
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        var names = new List<string> { "Krmivo pro drůbež", "Krmivo pro slepice", "Krmné obilí" };

        _mockPurchaseRepository.Setup(x => x.GetDistinctNamesByQueryAsync("krm", 20))
            .ReturnsAsync(names);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(3);
        result.Value.Should().Contain("Krmivo pro drůbež");
        result.Value.Should().Contain("Krmivo pro slepice");
        result.Value.Should().Contain("Krmné obilí");

        _mockPurchaseRepository.Verify(x => x.GetDistinctNamesByQueryAsync("krm", 20), Times.Once);
    }

    [Fact]
    public async Task Handle_WithCustomLimit_ShouldRespectLimit()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var query = new GetPurchaseNamesQuery
        {
            Query = "feed",
            Limit = 5
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        var names = new List<string> { "Feed 1", "Feed 2", "Feed 3", "Feed 4", "Feed 5" };

        _mockPurchaseRepository.Setup(x => x.GetDistinctNamesByQueryAsync("feed", 5))
            .ReturnsAsync(names);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(5);

        _mockPurchaseRepository.Verify(x => x.GetDistinctNamesByQueryAsync("feed", 5), Times.Once);
    }

    [Fact]
    public async Task Handle_WithDefaultLimit_ShouldUse20AsLimit()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var query = new GetPurchaseNamesQuery
        {
            Query = "test"
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        var names = new List<string> { "Test 1", "Test 2" };

        _mockPurchaseRepository.Setup(x => x.GetDistinctNamesByQueryAsync("test", 20))
            .ReturnsAsync(names);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _mockPurchaseRepository.Verify(x => x.GetDistinctNamesByQueryAsync("test", 20), Times.Once);
    }

    #endregion

    #region Empty Query Tests

    [Fact]
    public async Task Handle_WithNullQuery_ShouldReturnEmptyList()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var query = new GetPurchaseNamesQuery
        {
            Query = null
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();

        _mockPurchaseRepository.Verify(x => x.GetDistinctNamesByQueryAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithEmptyQuery_ShouldReturnEmptyList()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var query = new GetPurchaseNamesQuery
        {
            Query = ""
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();

        _mockPurchaseRepository.Verify(x => x.GetDistinctNamesByQueryAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithWhitespaceQuery_ShouldReturnEmptyList()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var query = new GetPurchaseNamesQuery
        {
            Query = "   "
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();

        _mockPurchaseRepository.Verify(x => x.GetDistinctNamesByQueryAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
    }

    #endregion

    #region No Matches Tests

    [Fact]
    public async Task Handle_WithNoMatchingNames_ShouldReturnEmptyList()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var query = new GetPurchaseNamesQuery
        {
            Query = "xyz123"
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        _mockPurchaseRepository.Setup(x => x.GetDistinctNamesByQueryAsync("xyz123", 20))
            .ReturnsAsync(new List<string>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }

    #endregion

    #region Authentication Tests

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldReturnUnauthorizedError()
    {
        // Arrange
        var query = new GetPurchaseNamesQuery
        {
            Query = "test"
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(false);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Unauthorized");
        result.Error.Message.Should().Be("User is not authenticated");

        _mockPurchaseRepository.Verify(x => x.GetDistinctNamesByQueryAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenTenantIdNotFound_ShouldReturnUnauthorizedError()
    {
        // Arrange
        var query = new GetPurchaseNamesQuery
        {
            Query = "test"
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns((Guid?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Unauthorized");
        result.Error.Message.Should().Be("Tenant not found");

        _mockPurchaseRepository.Verify(x => x.GetDistinctNamesByQueryAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldReturnFailureError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var query = new GetPurchaseNamesQuery
        {
            Query = "test"
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockPurchaseRepository.Setup(x => x.GetDistinctNamesByQueryAsync("test", 20))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Failure");
        result.Error.Message.Should().Contain("Failed to retrieve purchase names");
    }

    #endregion
}
