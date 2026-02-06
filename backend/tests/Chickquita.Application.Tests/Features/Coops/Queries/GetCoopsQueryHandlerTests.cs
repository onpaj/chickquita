using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using Chickquita.Application.DTOs;
using Chickquita.Application.Features.Coops.Queries;
using Chickquita.Application.Interfaces;
using Chickquita.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Chickquita.Application.Tests.Features.Coops.Queries;

/// <summary>
/// Unit tests for GetCoopsQueryHandler.
/// Tests cover retrieving coops, filtering by active status, tenant isolation, and sorting.
/// </summary>
public class GetCoopsQueryHandlerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<ICoopRepository> _mockCoopRepository;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<GetCoopsQueryHandler>> _mockLogger;
    private readonly GetCoopsQueryHandler _handler;

    public GetCoopsQueryHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());

        _mockCoopRepository = _fixture.Freeze<Mock<ICoopRepository>>();
        _mockCurrentUserService = _fixture.Freeze<Mock<ICurrentUserService>>();
        _mockMapper = _fixture.Freeze<Mock<IMapper>>();
        _mockLogger = _fixture.Freeze<Mock<ILogger<GetCoopsQueryHandler>>>();

        _handler = new GetCoopsQueryHandler(
            _mockCoopRepository.Object,
            _mockCurrentUserService.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    #region Happy Path Tests

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnAllActiveCoops()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var query = new GetCoopsQuery { IncludeArchived = false };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        var activeCoop1 = Coop.Create(tenantId, "Main Coop", "North Field");
        var activeCoop2 = Coop.Create(tenantId, "Secondary Coop", "South Field");
        var coops = new List<Coop> { activeCoop1, activeCoop2 };

        _mockCoopRepository.Setup(x => x.GetAllAsync(false))
            .ReturnsAsync(coops);

        var dto1 = new CoopDto
        {
            Id = activeCoop1.Id,
            TenantId = tenantId,
            Name = activeCoop1.Name,
            Location = activeCoop1.Location,
            IsActive = true,
            FlocksCount = 0
        };

        var dto2 = new CoopDto
        {
            Id = activeCoop2.Id,
            TenantId = tenantId,
            Name = activeCoop2.Name,
            Location = activeCoop2.Location,
            IsActive = true,
            FlocksCount = 0
        };

        _mockMapper.Setup(x => x.Map<List<CoopDto>>(coops))
            .Returns(new List<CoopDto> { dto1, dto2 });

        _mockCoopRepository.Setup(x => x.GetFlocksCountAsync(It.IsAny<Guid>()))
            .ReturnsAsync(0);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
        result.Value.Should().AllSatisfy(c => c.IsActive.Should().BeTrue());
        result.Value.Should().Contain(c => c.Name == "Main Coop");
        result.Value.Should().Contain(c => c.Name == "Secondary Coop");

        _mockCoopRepository.Verify(x => x.GetAllAsync(false), Times.Once);
    }

    [Fact]
    public async Task Handle_WithDefaultQuery_ShouldFilterArchivedCoops()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var query = new GetCoopsQuery(); // Default: IncludeArchived = false

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        var activeCoop = Coop.Create(tenantId, "Main Coop", "North Field");
        var coops = new List<Coop> { activeCoop };

        _mockCoopRepository.Setup(x => x.GetAllAsync(false))
            .ReturnsAsync(coops);

        var dto = new CoopDto
        {
            Id = activeCoop.Id,
            TenantId = tenantId,
            Name = activeCoop.Name,
            Location = activeCoop.Location,
            IsActive = true,
            FlocksCount = 0
        };

        _mockMapper.Setup(x => x.Map<List<CoopDto>>(coops))
            .Returns(new List<CoopDto> { dto });

        _mockCoopRepository.Setup(x => x.GetFlocksCountAsync(It.IsAny<Guid>()))
            .ReturnsAsync(0);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(1);
        result.Value.Should().AllSatisfy(c => c.IsActive.Should().BeTrue());

        // Verify that GetAllAsync was called with includeArchived = false
        _mockCoopRepository.Verify(x => x.GetAllAsync(false), Times.Once);
    }

    [Fact]
    public async Task Handle_WithIncludeArchivedTrue_ShouldReturnAllCoops()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var query = new GetCoopsQuery { IncludeArchived = true };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        var activeCoop = Coop.Create(tenantId, "Main Coop", "North Field");
        var archivedCoop = Coop.Create(tenantId, "Old Coop", "West Field");
        archivedCoop.Deactivate();

        var coops = new List<Coop> { activeCoop, archivedCoop };

        _mockCoopRepository.Setup(x => x.GetAllAsync(true))
            .ReturnsAsync(coops);

        var dto1 = new CoopDto
        {
            Id = activeCoop.Id,
            TenantId = tenantId,
            Name = activeCoop.Name,
            Location = activeCoop.Location,
            IsActive = true,
            FlocksCount = 0
        };

        var dto2 = new CoopDto
        {
            Id = archivedCoop.Id,
            TenantId = tenantId,
            Name = archivedCoop.Name,
            Location = archivedCoop.Location,
            IsActive = false,
            FlocksCount = 0
        };

        _mockMapper.Setup(x => x.Map<List<CoopDto>>(coops))
            .Returns(new List<CoopDto> { dto1, dto2 });

        _mockCoopRepository.Setup(x => x.GetFlocksCountAsync(It.IsAny<Guid>()))
            .ReturnsAsync(0);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
        result.Value.Should().Contain(c => c.IsActive == true);
        result.Value.Should().Contain(c => c.IsActive == false);

        // Verify that GetAllAsync was called with includeArchived = true
        _mockCoopRepository.Verify(x => x.GetAllAsync(true), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNoCoopsExist_ShouldReturnEmptyList()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var query = new GetCoopsQuery();

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        var emptyCoopList = new List<Coop>();

        _mockCoopRepository.Setup(x => x.GetAllAsync(false))
            .ReturnsAsync(emptyCoopList);

        _mockMapper.Setup(x => x.Map<List<CoopDto>>(emptyCoopList))
            .Returns(new List<CoopDto>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();

        _mockCoopRepository.Verify(x => x.GetAllAsync(false), Times.Once);
        _mockMapper.Verify(x => x.Map<List<CoopDto>>(emptyCoopList), Times.Once);
    }

    #endregion

    #region Tenant Isolation Tests

    [Fact]
    public async Task Handle_ShouldOnlyReturnCurrentTenantCoops()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var query = new GetCoopsQuery();

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        // The repository should handle tenant isolation via RLS and global query filters
        // so we only get back coops for this tenant
        var tenantCoop1 = Coop.Create(tenantId, "Tenant Coop 1", "Location 1");
        var tenantCoop2 = Coop.Create(tenantId, "Tenant Coop 2", "Location 2");
        var coops = new List<Coop> { tenantCoop1, tenantCoop2 };

        _mockCoopRepository.Setup(x => x.GetAllAsync(false))
            .ReturnsAsync(coops);

        var dto1 = new CoopDto
        {
            Id = tenantCoop1.Id,
            TenantId = tenantId,
            Name = tenantCoop1.Name,
            Location = tenantCoop1.Location,
            IsActive = true,
            FlocksCount = 0
        };

        var dto2 = new CoopDto
        {
            Id = tenantCoop2.Id,
            TenantId = tenantId,
            Name = tenantCoop2.Name,
            Location = tenantCoop2.Location,
            IsActive = true,
            FlocksCount = 0
        };

        _mockMapper.Setup(x => x.Map<List<CoopDto>>(coops))
            .Returns(new List<CoopDto> { dto1, dto2 });

        _mockCoopRepository.Setup(x => x.GetFlocksCountAsync(It.IsAny<Guid>()))
            .ReturnsAsync(0);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().AllSatisfy(c => c.TenantId.Should().Be(tenantId));

        _mockCoopRepository.Verify(x => x.GetAllAsync(false), Times.Once);
    }

    #endregion

    #region Sorting Tests

    [Fact]
    public async Task Handle_ShouldReturnCoopsSortedByCreationDateNewestFirst()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var query = new GetCoopsQuery();

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        // Create coops with different creation dates (repository returns them sorted)
        var olderCoop = Coop.Create(tenantId, "Older Coop", "Location 1");
        var newerCoop = Coop.Create(tenantId, "Newer Coop", "Location 2");

        // Repository should return coops sorted by CreatedAt descending (newest first)
        var coops = new List<Coop> { newerCoop, olderCoop };

        _mockCoopRepository.Setup(x => x.GetAllAsync(false))
            .ReturnsAsync(coops);

        var dto1 = new CoopDto
        {
            Id = newerCoop.Id,
            TenantId = tenantId,
            Name = newerCoop.Name,
            Location = newerCoop.Location,
            IsActive = true,
            CreatedAt = newerCoop.CreatedAt,
            FlocksCount = 0
        };

        var dto2 = new CoopDto
        {
            Id = olderCoop.Id,
            TenantId = tenantId,
            Name = olderCoop.Name,
            Location = olderCoop.Location,
            IsActive = true,
            CreatedAt = olderCoop.CreatedAt,
            FlocksCount = 0
        };

        _mockMapper.Setup(x => x.Map<List<CoopDto>>(coops))
            .Returns(new List<CoopDto> { dto1, dto2 });

        _mockCoopRepository.Setup(x => x.GetFlocksCountAsync(It.IsAny<Guid>()))
            .ReturnsAsync(0);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);

        // Verify the order is preserved (newest first)
        result.Value[0].Name.Should().Be("Newer Coop");
        result.Value[1].Name.Should().Be("Older Coop");

        _mockCoopRepository.Verify(x => x.GetAllAsync(false), Times.Once);
    }

    #endregion

    #region Flocks Count Tests

    [Fact]
    public async Task Handle_ShouldPopulateFlocksCountForEachCoop()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var query = new GetCoopsQuery();

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        var coop1 = Coop.Create(tenantId, "Coop 1", "Location 1");
        var coop2 = Coop.Create(tenantId, "Coop 2", "Location 2");
        var coops = new List<Coop> { coop1, coop2 };

        _mockCoopRepository.Setup(x => x.GetAllAsync(false))
            .ReturnsAsync(coops);

        var dto1 = new CoopDto
        {
            Id = coop1.Id,
            TenantId = tenantId,
            Name = coop1.Name,
            Location = coop1.Location,
            IsActive = true,
            FlocksCount = 0
        };

        var dto2 = new CoopDto
        {
            Id = coop2.Id,
            TenantId = tenantId,
            Name = coop2.Name,
            Location = coop2.Location,
            IsActive = true,
            FlocksCount = 0
        };

        _mockMapper.Setup(x => x.Map<List<CoopDto>>(coops))
            .Returns(new List<CoopDto> { dto1, dto2 });

        // Simulate different flock counts
        _mockCoopRepository.Setup(x => x.GetFlocksCountAsync(coop1.Id))
            .ReturnsAsync(3);
        _mockCoopRepository.Setup(x => x.GetFlocksCountAsync(coop2.Id))
            .ReturnsAsync(5);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);

        var resultCoop1 = result.Value.First(c => c.Id == coop1.Id);
        var resultCoop2 = result.Value.First(c => c.Id == coop2.Id);

        resultCoop1.FlocksCount.Should().Be(3);
        resultCoop2.FlocksCount.Should().Be(5);

        _mockCoopRepository.Verify(x => x.GetFlocksCountAsync(coop1.Id), Times.Once);
        _mockCoopRepository.Verify(x => x.GetFlocksCountAsync(coop2.Id), Times.Once);
    }

    #endregion

    #region Authentication Tests

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldReturnUnauthorizedError()
    {
        // Arrange
        var query = new GetCoopsQuery();

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(false);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Unauthorized");
        result.Error.Message.Should().Be("User is not authenticated");

        _mockCoopRepository.Verify(x => x.GetAllAsync(It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenTenantIdNotFound_ShouldReturnUnauthorizedError()
    {
        // Arrange
        var query = new GetCoopsQuery();

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns((Guid?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Unauthorized");
        result.Error.Message.Should().Be("Tenant not found");

        _mockCoopRepository.Verify(x => x.GetAllAsync(It.IsAny<bool>()), Times.Never);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldReturnFailureError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var query = new GetCoopsQuery();

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        _mockCoopRepository.Setup(x => x.GetAllAsync(false))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Failure");
        result.Error.Message.Should().Contain("Failed to retrieve coops");
        result.Error.Message.Should().Contain("Database connection failed");
    }

    [Fact]
    public async Task Handle_WhenMapperThrowsException_ShouldReturnFailureError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var query = new GetCoopsQuery();

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        var coops = new List<Coop> { Coop.Create(tenantId, "Test Coop", "Test Location") };

        _mockCoopRepository.Setup(x => x.GetAllAsync(false))
            .ReturnsAsync(coops);

        _mockMapper.Setup(x => x.Map<List<CoopDto>>(It.IsAny<List<Coop>>()))
            .Throws(new Exception("Mapping failed"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Failure");
        result.Error.Message.Should().Contain("Failed to retrieve coops");
        result.Error.Message.Should().Contain("Mapping failed");
    }

    [Fact]
    public async Task Handle_WhenGetFlocksCountThrowsException_ShouldReturnFailureError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var query = new GetCoopsQuery();

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        var coop = Coop.Create(tenantId, "Test Coop", "Test Location");
        var coops = new List<Coop> { coop };

        _mockCoopRepository.Setup(x => x.GetAllAsync(false))
            .ReturnsAsync(coops);

        var dto = new CoopDto
        {
            Id = coop.Id,
            TenantId = tenantId,
            Name = coop.Name,
            Location = coop.Location,
            IsActive = true,
            FlocksCount = 0
        };

        _mockMapper.Setup(x => x.Map<List<CoopDto>>(coops))
            .Returns(new List<CoopDto> { dto });

        _mockCoopRepository.Setup(x => x.GetFlocksCountAsync(coop.Id))
            .ThrowsAsync(new Exception("Failed to get flocks count"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Failure");
        result.Error.Message.Should().Contain("Failed to retrieve coops");
        result.Error.Message.Should().Contain("Failed to get flocks count");
    }

    #endregion
}
