using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using Chickquita.Application.DTOs;
using Chickquita.Application.Features.Flocks.Queries;
using Chickquita.Application.Interfaces;
using Chickquita.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Chickquita.Application.Tests.Features.Flocks.Queries;

/// <summary>
/// Unit tests for GetFlocksQueryHandler.
/// Tests cover filtering by coop ID, active/archived status, tenant isolation, and ordering.
/// </summary>
public class GetFlocksQueryHandlerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IFlockRepository> _mockFlockRepository;
    private readonly Mock<ICoopRepository> _mockCoopRepository;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<GetFlocksQueryHandler>> _mockLogger;
    private readonly GetFlocksQueryHandler _handler;

    public GetFlocksQueryHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());

        _mockFlockRepository = _fixture.Freeze<Mock<IFlockRepository>>();
        _mockCoopRepository = _fixture.Freeze<Mock<ICoopRepository>>();
        _mockCurrentUserService = _fixture.Freeze<Mock<ICurrentUserService>>();
        _mockMapper = _fixture.Freeze<Mock<IMapper>>();
        _mockLogger = _fixture.Freeze<Mock<ILogger<GetFlocksQueryHandler>>>();

        _handler = new GetFlocksQueryHandler(
            _mockFlockRepository.Object,
            _mockCoopRepository.Object,
            _mockCurrentUserService.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    #region Happy Path Tests

    [Fact]
    public async Task Handle_WithValidCoopId_ShouldReturnFlocksForCoop()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var query = new GetFlocksQuery
        {
            CoopId = coopId,
            IncludeInactive = false
        };

        var coop = Coop.Create(tenantId, "Main Coop", "North Field");
        var flock1 = Flock.Create(tenantId, coopId, "Spring 2024", DateTime.UtcNow.AddDays(-60), 10, 2, 5);
        var flock2 = Flock.Create(tenantId, coopId, "Winter 2024", DateTime.UtcNow.AddDays(-30), 8, 1, 3);

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockCoopRepository.Setup(x => x.GetByIdAsync(coopId)).ReturnsAsync(coop);
        _mockFlockRepository.Setup(x => x.GetByCoopIdAsync(coopId, false))
            .ReturnsAsync(new List<Flock> { flock1, flock2 });

        var flockDtos = new List<FlockDto>
        {
            new FlockDto { Id = flock1.Id, Identifier = "Spring 2024", CreatedAt = DateTime.UtcNow.AddDays(-60) },
            new FlockDto { Id = flock2.Id, Identifier = "Winter 2024", CreatedAt = DateTime.UtcNow.AddDays(-30) }
        };

        _mockMapper.Setup(x => x.Map<List<FlockDto>>(It.IsAny<List<Flock>>()))
            .Returns(flockDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);

        // Verify ordering by CreatedAt descending (newest first)
        result.Value[0].CreatedAt.Should().BeAfter(result.Value[1].CreatedAt);

        _mockCoopRepository.Verify(x => x.GetByIdAsync(coopId), Times.Once);
        _mockFlockRepository.Verify(x => x.GetByCoopIdAsync(coopId, false), Times.Once);
        _mockMapper.Verify(x => x.Map<List<FlockDto>>(It.IsAny<List<Flock>>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithoutCoopId_ShouldReturnAllFlocksForTenant()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var query = new GetFlocksQuery
        {
            CoopId = null,
            IncludeInactive = false
        };

        var flock1 = Flock.Create(tenantId, Guid.NewGuid(), "Flock A", DateTime.UtcNow.AddDays(-60), 10, 2, 5);
        var flock2 = Flock.Create(tenantId, Guid.NewGuid(), "Flock B", DateTime.UtcNow.AddDays(-30), 8, 1, 3);

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockFlockRepository.Setup(x => x.GetAllAsync(false))
            .ReturnsAsync(new List<Flock> { flock1, flock2 });

        var flockDtos = new List<FlockDto>
        {
            new FlockDto { Id = flock1.Id, Identifier = "Flock A", CreatedAt = DateTime.UtcNow.AddDays(-60) },
            new FlockDto { Id = flock2.Id, Identifier = "Flock B", CreatedAt = DateTime.UtcNow.AddDays(-30) }
        };

        _mockMapper.Setup(x => x.Map<List<FlockDto>>(It.IsAny<List<Flock>>()))
            .Returns(flockDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);

        // Verify ordering by CreatedAt descending (newest first)
        result.Value[0].CreatedAt.Should().BeAfter(result.Value[1].CreatedAt);

        _mockCoopRepository.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _mockFlockRepository.Verify(x => x.GetAllAsync(false), Times.Once);
        _mockMapper.Verify(x => x.Map<List<FlockDto>>(It.IsAny<List<Flock>>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyResult_ShouldReturnEmptyList()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var query = new GetFlocksQuery
        {
            CoopId = coopId,
            IncludeInactive = false
        };

        var coop = Coop.Create(tenantId, "Main Coop", "North Field");

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockCoopRepository.Setup(x => x.GetByIdAsync(coopId)).ReturnsAsync(coop);
        _mockFlockRepository.Setup(x => x.GetByCoopIdAsync(coopId, false))
            .ReturnsAsync(new List<Flock>());

        _mockMapper.Setup(x => x.Map<List<FlockDto>>(It.IsAny<List<Flock>>()))
            .Returns(new List<FlockDto>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();

        _mockCoopRepository.Verify(x => x.GetByIdAsync(coopId), Times.Once);
        _mockFlockRepository.Verify(x => x.GetByCoopIdAsync(coopId, false), Times.Once);
    }

    #endregion

    #region Filtering Tests

    [Fact]
    public async Task Handle_WithIncludeInactiveFalse_ShouldFilterArchivedFlocks()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var query = new GetFlocksQuery
        {
            CoopId = coopId,
            IncludeInactive = false
        };

        var coop = Coop.Create(tenantId, "Main Coop", "North Field");
        var activeFlock = Flock.Create(tenantId, coopId, "Active Flock", DateTime.UtcNow.AddDays(-30), 10, 2, 5);

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockCoopRepository.Setup(x => x.GetByIdAsync(coopId)).ReturnsAsync(coop);
        _mockFlockRepository.Setup(x => x.GetByCoopIdAsync(coopId, false))
            .ReturnsAsync(new List<Flock> { activeFlock });

        var flockDtos = new List<FlockDto>
        {
            new FlockDto { Id = activeFlock.Id, Identifier = "Active Flock", IsActive = true, CreatedAt = DateTime.UtcNow.AddDays(-30) }
        };

        _mockMapper.Setup(x => x.Map<List<FlockDto>>(It.IsAny<List<Flock>>()))
            .Returns(flockDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(1);
        result.Value.Should().AllSatisfy(f => f.IsActive.Should().BeTrue());

        _mockFlockRepository.Verify(x => x.GetByCoopIdAsync(coopId, false), Times.Once);
    }

    [Fact]
    public async Task Handle_WithIncludeInactiveTrue_ShouldIncludeArchivedFlocks()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var query = new GetFlocksQuery
        {
            CoopId = coopId,
            IncludeInactive = true
        };

        var coop = Coop.Create(tenantId, "Main Coop", "North Field");
        var activeFlock = Flock.Create(tenantId, coopId, "Active Flock", DateTime.UtcNow.AddDays(-30), 10, 2, 5);
        var archivedFlock = Flock.Create(tenantId, coopId, "Archived Flock", DateTime.UtcNow.AddDays(-90), 8, 1, 3);
        archivedFlock.Archive();

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockCoopRepository.Setup(x => x.GetByIdAsync(coopId)).ReturnsAsync(coop);
        _mockFlockRepository.Setup(x => x.GetByCoopIdAsync(coopId, true))
            .ReturnsAsync(new List<Flock> { activeFlock, archivedFlock });

        var flockDtos = new List<FlockDto>
        {
            new FlockDto { Id = activeFlock.Id, Identifier = "Active Flock", IsActive = true, CreatedAt = DateTime.UtcNow.AddDays(-30) },
            new FlockDto { Id = archivedFlock.Id, Identifier = "Archived Flock", IsActive = false, CreatedAt = DateTime.UtcNow.AddDays(-90) }
        };

        _mockMapper.Setup(x => x.Map<List<FlockDto>>(It.IsAny<List<Flock>>()))
            .Returns(flockDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
        result.Value.Should().Contain(f => f.IsActive == true);
        result.Value.Should().Contain(f => f.IsActive == false);

        _mockFlockRepository.Verify(x => x.GetByCoopIdAsync(coopId, true), Times.Once);
    }

    #endregion

    #region Ordering Tests

    [Fact]
    public async Task Handle_ShouldReturnFlocksOrderedByCreatedAtDescending()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var query = new GetFlocksQuery
        {
            CoopId = coopId,
            IncludeInactive = false
        };

        var coop = Coop.Create(tenantId, "Main Coop", "North Field");
        var oldestFlock = Flock.Create(tenantId, coopId, "Oldest", DateTime.UtcNow.AddDays(-90), 10, 2, 5);
        var middleFlock = Flock.Create(tenantId, coopId, "Middle", DateTime.UtcNow.AddDays(-60), 8, 1, 3);
        var newestFlock = Flock.Create(tenantId, coopId, "Newest", DateTime.UtcNow.AddDays(-30), 12, 2, 6);

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockCoopRepository.Setup(x => x.GetByIdAsync(coopId)).ReturnsAsync(coop);
        _mockFlockRepository.Setup(x => x.GetByCoopIdAsync(coopId, false))
            .ReturnsAsync(new List<Flock> { oldestFlock, middleFlock, newestFlock });

        var flockDtos = new List<FlockDto>
        {
            new FlockDto { Id = oldestFlock.Id, Identifier = "Oldest", CreatedAt = DateTime.UtcNow.AddDays(-90) },
            new FlockDto { Id = middleFlock.Id, Identifier = "Middle", CreatedAt = DateTime.UtcNow.AddDays(-60) },
            new FlockDto { Id = newestFlock.Id, Identifier = "Newest", CreatedAt = DateTime.UtcNow.AddDays(-30) }
        };

        _mockMapper.Setup(x => x.Map<List<FlockDto>>(It.IsAny<List<Flock>>()))
            .Returns(flockDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(3);

        // Verify ordering: newest first
        result.Value[0].Identifier.Should().Be("Newest");
        result.Value[1].Identifier.Should().Be("Middle");
        result.Value[2].Identifier.Should().Be("Oldest");

        // Verify descending order
        for (int i = 0; i < result.Value.Count - 1; i++)
        {
            result.Value[i].CreatedAt.Should().BeAfter(result.Value[i + 1].CreatedAt);
        }
    }

    #endregion

    #region Coop Validation Tests

    [Fact]
    public async Task Handle_WhenCoopDoesNotExist_ShouldReturnNotFoundError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var query = new GetFlocksQuery
        {
            CoopId = coopId,
            IncludeInactive = false
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockCoopRepository.Setup(x => x.GetByIdAsync(coopId))
            .ReturnsAsync((Coop?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.NotFound");
        result.Error.Message.Should().Be("Coop not found");

        _mockCoopRepository.Verify(x => x.GetByIdAsync(coopId), Times.Once);
        _mockFlockRepository.Verify(x => x.GetByCoopIdAsync(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Never);
    }

    #endregion

    #region Authentication and Tenant Isolation Tests

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldReturnUnauthorizedError()
    {
        // Arrange
        var query = new GetFlocksQuery
        {
            CoopId = Guid.NewGuid(),
            IncludeInactive = false
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(false);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Unauthorized");
        result.Error.Message.Should().Be("User is not authenticated");

        _mockCoopRepository.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _mockFlockRepository.Verify(x => x.GetByCoopIdAsync(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Never);
        _mockFlockRepository.Verify(x => x.GetAllAsync(It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenTenantIdNotFound_ShouldReturnUnauthorizedError()
    {
        // Arrange
        var query = new GetFlocksQuery
        {
            CoopId = Guid.NewGuid(),
            IncludeInactive = false
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

        _mockCoopRepository.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _mockFlockRepository.Verify(x => x.GetByCoopIdAsync(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Never);
        _mockFlockRepository.Verify(x => x.GetAllAsync(It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldOnlyAccessCurrentTenantData()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var query = new GetFlocksQuery
        {
            CoopId = coopId,
            IncludeInactive = false
        };

        // Coop belongs to current tenant
        var coop = Coop.Create(tenantId, "Main Coop", "North Field");

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockCoopRepository.Setup(x => x.GetByIdAsync(coopId)).ReturnsAsync(coop);
        _mockFlockRepository.Setup(x => x.GetByCoopIdAsync(coopId, false))
            .ReturnsAsync(new List<Flock>());

        _mockMapper.Setup(x => x.Map<List<FlockDto>>(It.IsAny<List<Flock>>()))
            .Returns(new List<FlockDto>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify coop verification happened (ensures tenant isolation at coop level)
        _mockCoopRepository.Verify(x => x.GetByIdAsync(coopId), Times.Once);

        // Repository should apply tenant filtering via RLS/EF Core global filters
        _mockFlockRepository.Verify(x => x.GetByCoopIdAsync(coopId, false), Times.Once);
    }

    #endregion

    #region DTO Mapping Tests

    [Fact]
    public async Task Handle_ShouldMapToFlockDtoWithAllRequiredFields()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var query = new GetFlocksQuery
        {
            CoopId = coopId,
            IncludeInactive = false
        };

        var coop = Coop.Create(tenantId, "Main Coop", "North Field");
        var flock = Flock.Create(tenantId, coopId, "Test Flock", DateTime.UtcNow.AddDays(-30), 10, 2, 5);

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockCoopRepository.Setup(x => x.GetByIdAsync(coopId)).ReturnsAsync(coop);
        _mockFlockRepository.Setup(x => x.GetByCoopIdAsync(coopId, false))
            .ReturnsAsync(new List<Flock> { flock });

        var expectedDto = new FlockDto
        {
            Id = flock.Id,
            CoopId = coopId,
            Identifier = "Test Flock",
            HatchDate = flock.HatchDate,
            CurrentHens = 10,
            CurrentRoosters = 2,
            CurrentChicks = 5,
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            UpdatedAt = DateTime.UtcNow
        };

        _mockMapper.Setup(x => x.Map<List<FlockDto>>(It.IsAny<List<Flock>>()))
            .Returns(new List<FlockDto> { expectedDto });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(1);

        var dto = result.Value[0];
        dto.Id.Should().Be(expectedDto.Id);
        dto.CoopId.Should().Be(expectedDto.CoopId);
        dto.Identifier.Should().Be(expectedDto.Identifier);
        dto.HatchDate.Should().Be(expectedDto.HatchDate);
        dto.CurrentHens.Should().Be(expectedDto.CurrentHens);
        dto.CurrentRoosters.Should().Be(expectedDto.CurrentRoosters);
        dto.CurrentChicks.Should().Be(expectedDto.CurrentChicks);
        dto.IsActive.Should().Be(expectedDto.IsActive);
        dto.CreatedAt.Should().Be(expectedDto.CreatedAt);
        dto.UpdatedAt.Should().Be(expectedDto.UpdatedAt);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldReturnFailureError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var query = new GetFlocksQuery
        {
            CoopId = coopId,
            IncludeInactive = false
        };

        var coop = Coop.Create(tenantId, "Main Coop", "North Field");
        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockCoopRepository.Setup(x => x.GetByIdAsync(coopId)).ReturnsAsync(coop);
        _mockFlockRepository.Setup(x => x.GetByCoopIdAsync(coopId, false))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Failure");
        result.Error.Message.Should().Contain("Failed to retrieve flocks");
    }

    #endregion
}
