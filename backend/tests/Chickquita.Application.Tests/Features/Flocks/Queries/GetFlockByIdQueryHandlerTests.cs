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
/// Unit tests for GetFlockByIdQueryHandler.
/// Tests cover successful retrieval, flock not found, tenant isolation, and DTO mapping.
/// </summary>
public class GetFlockByIdQueryHandlerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IFlockRepository> _mockFlockRepository;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<GetFlockByIdQueryHandler>> _mockLogger;
    private readonly GetFlockByIdQueryHandler _handler;

    public GetFlockByIdQueryHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());

        _mockFlockRepository = _fixture.Freeze<Mock<IFlockRepository>>();
        _mockCurrentUserService = _fixture.Freeze<Mock<ICurrentUserService>>();
        _mockMapper = _fixture.Freeze<Mock<IMapper>>();
        _mockLogger = _fixture.Freeze<Mock<ILogger<GetFlockByIdQueryHandler>>>();

        _handler = new GetFlockByIdQueryHandler(
            _mockFlockRepository.Object,
            _mockCurrentUserService.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    #region Happy Path Tests

    [Fact]
    public async Task Handle_WithValidFlockId_ShouldReturnFlockDto()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var flockId = Guid.NewGuid();
        var query = new GetFlockByIdQuery { FlockId = flockId };

        var flock = Flock.Create(tenantId, coopId, "Spring 2024", DateTime.UtcNow.AddDays(-60), 10, 2, 5);

        var expectedDto = new FlockDto
        {
            Id = flock.Id,
            TenantId = tenantId,
            CoopId = coopId,
            Identifier = "Spring 2024",
            HatchDate = flock.HatchDate,
            CurrentHens = 10,
            CurrentRoosters = 2,
            CurrentChicks = 5,
            IsActive = true,
            CreatedAt = flock.CreatedAt,
            UpdatedAt = flock.UpdatedAt,
            History = new List<FlockHistoryDto>()
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockFlockRepository.Setup(x => x.GetByIdAsync(flockId)).ReturnsAsync(flock);
        _mockMapper.Setup(x => x.Map<FlockDto>(flock)).Returns(expectedDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(flock.Id);
        result.Value.Identifier.Should().Be("Spring 2024");
        result.Value.CurrentHens.Should().Be(10);
        result.Value.CurrentRoosters.Should().Be(2);
        result.Value.CurrentChicks.Should().Be(5);
        result.Value.IsActive.Should().BeTrue();

        _mockFlockRepository.Verify(x => x.GetByIdAsync(flockId), Times.Once);
        _mockMapper.Verify(x => x.Map<FlockDto>(flock), Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidFlockId_ShouldIncludeFlockHistory()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var flockId = Guid.NewGuid();
        var query = new GetFlockByIdQuery { FlockId = flockId };

        var flock = Flock.Create(tenantId, coopId, "Spring 2024", DateTime.UtcNow.AddDays(-60), 10, 2, 5);

        var historyDto1 = new FlockHistoryDto
        {
            Id = Guid.NewGuid(),
            ChangeDate = DateTime.UtcNow.AddDays(-30),
            Hens = 12,
            Roosters = 2,
            Chicks = 0,
            Reason = "Maturation",
            Notes = "Chicks matured to hens"
        };

        var historyDto2 = new FlockHistoryDto
        {
            Id = Guid.NewGuid(),
            ChangeDate = DateTime.UtcNow.AddDays(-60),
            Hens = 10,
            Roosters = 2,
            Chicks = 5,
            Reason = "Initial",
            Notes = "Initial flock composition"
        };

        var expectedDto = new FlockDto
        {
            Id = flock.Id,
            TenantId = tenantId,
            CoopId = coopId,
            Identifier = "Spring 2024",
            HatchDate = flock.HatchDate,
            CurrentHens = 12,
            CurrentRoosters = 2,
            CurrentChicks = 0,
            IsActive = true,
            CreatedAt = flock.CreatedAt,
            UpdatedAt = flock.UpdatedAt,
            History = new List<FlockHistoryDto> { historyDto1, historyDto2 }
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockFlockRepository.Setup(x => x.GetByIdAsync(flockId)).ReturnsAsync(flock);
        _mockMapper.Setup(x => x.Map<FlockDto>(flock)).Returns(expectedDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.History.Should().NotBeNull();
        result.Value.History.Should().HaveCount(2);
        result.Value.History[0].ChangeDate.Should().BeAfter(result.Value.History[1].ChangeDate);

        _mockFlockRepository.Verify(x => x.GetByIdAsync(flockId), Times.Once);
        _mockMapper.Verify(x => x.Map<FlockDto>(flock), Times.Once);
    }

    #endregion

    #region Flock Not Found Tests

    [Fact]
    public async Task Handle_WhenFlockDoesNotExist_ShouldReturnNotFoundError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var flockId = Guid.NewGuid();
        var query = new GetFlockByIdQuery { FlockId = flockId };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockFlockRepository.Setup(x => x.GetByIdAsync(flockId))
            .ReturnsAsync((Flock?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.NotFound");
        result.Error.Message.Should().Be("Flock not found");

        _mockFlockRepository.Verify(x => x.GetByIdAsync(flockId), Times.Once);
        _mockMapper.Verify(x => x.Map<FlockDto>(It.IsAny<Flock>()), Times.Never);
    }

    #endregion

    #region Tenant Isolation Tests

    [Fact]
    public async Task Handle_WhenFlockBelongsToDifferentTenant_ShouldReturnNotFoundError()
    {
        // Arrange
        var currentTenantId = Guid.NewGuid();
        var differentTenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var flockId = Guid.NewGuid();
        var query = new GetFlockByIdQuery { FlockId = flockId };

        // Repository returns null due to RLS/global query filter when flock belongs to different tenant
        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(currentTenantId);
        _mockFlockRepository.Setup(x => x.GetByIdAsync(flockId))
            .ReturnsAsync((Flock?)null); // RLS filters out the flock from different tenant

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.NotFound");
        result.Error.Message.Should().Be("Flock not found");

        _mockFlockRepository.Verify(x => x.GetByIdAsync(flockId), Times.Once);
        _mockMapper.Verify(x => x.Map<FlockDto>(It.IsAny<Flock>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldOnlyAccessCurrentTenantData()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var flockId = Guid.NewGuid();
        var query = new GetFlockByIdQuery { FlockId = flockId };

        var flock = Flock.Create(tenantId, coopId, "Test Flock", DateTime.UtcNow.AddDays(-30), 10, 2, 5);

        var expectedDto = new FlockDto
        {
            Id = flock.Id,
            TenantId = tenantId,
            CoopId = coopId,
            Identifier = "Test Flock"
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockFlockRepository.Setup(x => x.GetByIdAsync(flockId)).ReturnsAsync(flock);
        _mockMapper.Setup(x => x.Map<FlockDto>(flock)).Returns(expectedDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TenantId.Should().Be(tenantId);

        // Verify repository is called (which applies RLS/global filter)
        _mockFlockRepository.Verify(x => x.GetByIdAsync(flockId), Times.Once);
    }

    #endregion

    #region Authentication Tests

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldReturnUnauthorizedError()
    {
        // Arrange
        var flockId = Guid.NewGuid();
        var query = new GetFlockByIdQuery { FlockId = flockId };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(false);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Unauthorized");
        result.Error.Message.Should().Be("User is not authenticated");

        _mockFlockRepository.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _mockMapper.Verify(x => x.Map<FlockDto>(It.IsAny<Flock>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenTenantIdNotFound_ShouldReturnUnauthorizedError()
    {
        // Arrange
        var flockId = Guid.NewGuid();
        var query = new GetFlockByIdQuery { FlockId = flockId };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns((Guid?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Unauthorized");
        result.Error.Message.Should().Be("Tenant not found");

        _mockFlockRepository.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _mockMapper.Verify(x => x.Map<FlockDto>(It.IsAny<Flock>()), Times.Never);
    }

    #endregion

    #region DTO Mapping Tests

    [Fact]
    public async Task Handle_ShouldMapToFlockDtoWithAllRequiredFields()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var flockId = Guid.NewGuid();
        var query = new GetFlockByIdQuery { FlockId = flockId };

        var flock = Flock.Create(tenantId, coopId, "Test Flock", DateTime.UtcNow.AddDays(-30), 10, 2, 5);

        var expectedDto = new FlockDto
        {
            Id = flock.Id,
            TenantId = tenantId,
            CoopId = coopId,
            Identifier = "Test Flock",
            HatchDate = flock.HatchDate,
            CurrentHens = 10,
            CurrentRoosters = 2,
            CurrentChicks = 5,
            IsActive = true,
            CreatedAt = flock.CreatedAt,
            UpdatedAt = flock.UpdatedAt,
            History = new List<FlockHistoryDto>()
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockFlockRepository.Setup(x => x.GetByIdAsync(flockId)).ReturnsAsync(flock);
        _mockMapper.Setup(x => x.Map<FlockDto>(flock)).Returns(expectedDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        var dto = result.Value;
        dto.Id.Should().Be(expectedDto.Id);
        dto.TenantId.Should().Be(expectedDto.TenantId);
        dto.CoopId.Should().Be(expectedDto.CoopId);
        dto.Identifier.Should().Be(expectedDto.Identifier);
        dto.HatchDate.Should().Be(expectedDto.HatchDate);
        dto.CurrentHens.Should().Be(expectedDto.CurrentHens);
        dto.CurrentRoosters.Should().Be(expectedDto.CurrentRoosters);
        dto.CurrentChicks.Should().Be(expectedDto.CurrentChicks);
        dto.IsActive.Should().Be(expectedDto.IsActive);
        dto.CreatedAt.Should().Be(expectedDto.CreatedAt);
        dto.UpdatedAt.Should().Be(expectedDto.UpdatedAt);
        dto.History.Should().NotBeNull();
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldReturnFailureError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var flockId = Guid.NewGuid();
        var query = new GetFlockByIdQuery { FlockId = flockId };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockFlockRepository.Setup(x => x.GetByIdAsync(flockId))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Failure");
        result.Error.Message.Should().Contain("Failed to retrieve flock");
        result.Error.Message.Should().Contain("Database connection failed");

        _mockFlockRepository.Verify(x => x.GetByIdAsync(flockId), Times.Once);
        _mockMapper.Verify(x => x.Map<FlockDto>(It.IsAny<Flock>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenMapperThrowsException_ShouldReturnFailureError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var flockId = Guid.NewGuid();
        var query = new GetFlockByIdQuery { FlockId = flockId };

        var flock = Flock.Create(tenantId, coopId, "Test Flock", DateTime.UtcNow.AddDays(-30), 10, 2, 5);

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockFlockRepository.Setup(x => x.GetByIdAsync(flockId)).ReturnsAsync(flock);
        _mockMapper.Setup(x => x.Map<FlockDto>(flock))
            .Throws(new Exception("Mapping failed"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Failure");
        result.Error.Message.Should().Contain("Failed to retrieve flock");
        result.Error.Message.Should().Contain("Mapping failed");

        _mockFlockRepository.Verify(x => x.GetByIdAsync(flockId), Times.Once);
        _mockMapper.Verify(x => x.Map<FlockDto>(flock), Times.Once);
    }

    #endregion
}
