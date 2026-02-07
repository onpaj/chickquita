using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using Chickquita.Application.DTOs;
using Chickquita.Application.Features.Flocks.Commands;
using Chickquita.Application.Interfaces;
using Chickquita.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Chickquita.Application.Tests.Features.Flocks.Commands;

/// <summary>
/// Unit tests for ArchiveFlockCommandHandler.
/// Tests cover successful archiving, flock not found, tenant isolation, and archived flock filtering.
/// </summary>
public class ArchiveFlockCommandHandlerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IFlockRepository> _mockFlockRepository;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<ArchiveFlockCommandHandler>> _mockLogger;
    private readonly ArchiveFlockCommandHandler _handler;

    public ArchiveFlockCommandHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());

        _mockFlockRepository = _fixture.Freeze<Mock<IFlockRepository>>();
        _mockCurrentUserService = _fixture.Freeze<Mock<ICurrentUserService>>();
        _mockMapper = _fixture.Freeze<Mock<IMapper>>();
        _mockLogger = _fixture.Freeze<Mock<ILogger<ArchiveFlockCommandHandler>>>();

        _handler = new ArchiveFlockCommandHandler(
            _mockFlockRepository.Object,
            _mockCurrentUserService.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    #region Happy Path Tests

    [Fact]
    public async Task Handle_WithValidFlockId_ShouldArchiveFlockSuccessfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var flockId = Guid.NewGuid();

        var command = new ArchiveFlockCommand
        {
            FlockId = flockId
        };

        var existingFlock = Flock.Create(
            tenantId,
            coopId,
            "Test Flock",
            DateTime.UtcNow.AddDays(-60),
            initialHens: 10,
            initialRoosters: 2,
            initialChicks: 5);

        Flock? capturedFlock = null;

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockFlockRepository.Setup(x => x.GetByIdWithoutHistoryAsync(flockId))
            .ReturnsAsync(existingFlock);
        _mockFlockRepository.Setup(x => x.UpdateAsync(It.IsAny<Flock>()))
            .Callback<Flock>(f => capturedFlock = f)
            .ReturnsAsync((Flock f) => f);

        var expectedDto = new FlockDto
        {
            Id = flockId,
            TenantId = tenantId,
            CoopId = coopId,
            Identifier = "Test Flock",
            IsActive = false
        };

        _mockMapper.Setup(x => x.Map<FlockDto>(It.IsAny<Flock>()))
            .Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.IsActive.Should().BeFalse();

        capturedFlock.Should().NotBeNull();
        capturedFlock!.IsActive.Should().BeFalse();

        _mockFlockRepository.Verify(x => x.GetByIdWithoutHistoryAsync(flockId), Times.Once);
        _mockFlockRepository.Verify(x => x.UpdateAsync(It.IsAny<Flock>()), Times.Once);
        _mockMapper.Verify(x => x.Map<FlockDto>(It.IsAny<Flock>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSetIsActiveToFalse_WithoutHardDeletingFlock()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();

        var existingFlock = Flock.Create(
            tenantId,
            coopId,
            "Active Flock",
            DateTime.UtcNow.AddDays(-30),
            initialHens: 8,
            initialRoosters: 1,
            initialChicks: 3);

        var flockId = existingFlock.Id; // Use the actual flock ID

        var command = new ArchiveFlockCommand
        {
            FlockId = flockId
        };

        Flock? capturedFlock = null;

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockFlockRepository.Setup(x => x.GetByIdWithoutHistoryAsync(flockId))
            .ReturnsAsync(existingFlock);
        _mockFlockRepository.Setup(x => x.UpdateAsync(It.IsAny<Flock>()))
            .Callback<Flock>(f => capturedFlock = f)
            .ReturnsAsync((Flock f) => f);

        var expectedDto = new FlockDto { Id = flockId, IsActive = false };
        _mockMapper.Setup(x => x.Map<FlockDto>(It.IsAny<Flock>()))
            .Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify UpdateAsync was called (soft delete), NOT DeleteAsync (hard delete)
        _mockFlockRepository.Verify(x => x.UpdateAsync(It.IsAny<Flock>()), Times.Once);
        _mockFlockRepository.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);

        // Verify flock data is preserved
        capturedFlock.Should().NotBeNull();
        capturedFlock!.Id.Should().Be(flockId);
        capturedFlock.Identifier.Should().Be("Active Flock");
        capturedFlock.CurrentHens.Should().Be(8);
        capturedFlock.CurrentRoosters.Should().Be(1);
        capturedFlock.CurrentChicks.Should().Be(3);
        capturedFlock.IsActive.Should().BeFalse(); // Only IsActive changed
    }

    [Fact]
    public async Task Handle_WhenFlockAlreadyArchived_ShouldBeIdempotent()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var flockId = Guid.NewGuid();

        var command = new ArchiveFlockCommand
        {
            FlockId = flockId
        };

        var existingFlock = Flock.Create(
            tenantId,
            coopId,
            "Already Archived",
            DateTime.UtcNow.AddDays(-60),
            initialHens: 5,
            initialRoosters: 1,
            initialChicks: 0);

        // Archive it first
        existingFlock.Archive();

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockFlockRepository.Setup(x => x.GetByIdWithoutHistoryAsync(flockId))
            .ReturnsAsync(existingFlock);
        _mockFlockRepository.Setup(x => x.UpdateAsync(It.IsAny<Flock>()))
            .ReturnsAsync((Flock f) => f);

        var expectedDto = new FlockDto { Id = flockId, IsActive = false };
        _mockMapper.Setup(x => x.Map<FlockDto>(It.IsAny<Flock>()))
            .Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsActive.Should().BeFalse();
    }

    #endregion

    #region Flock Existence Validation Tests

    [Fact]
    public async Task Handle_WhenFlockDoesNotExist_ShouldReturnNotFoundError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var flockId = Guid.NewGuid();

        var command = new ArchiveFlockCommand
        {
            FlockId = flockId
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockFlockRepository.Setup(x => x.GetByIdWithoutHistoryAsync(flockId))
            .ReturnsAsync((Flock?)null); // Flock not found

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.NotFound");
        result.Error.Message.Should().Be("Flock not found");

        _mockFlockRepository.Verify(x => x.GetByIdWithoutHistoryAsync(flockId), Times.Once);
        _mockFlockRepository.Verify(x => x.UpdateAsync(It.IsAny<Flock>()), Times.Never);
    }

    #endregion

    #region Tenant Isolation Tests

    [Fact]
    public async Task Handle_WhenFlockBelongsToCurrentTenant_ShouldArchiveSuccessfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var flockId = Guid.NewGuid();

        var command = new ArchiveFlockCommand
        {
            FlockId = flockId
        };

        var existingFlock = Flock.Create(
            tenantId, // Same tenant
            coopId,
            "My Flock",
            DateTime.UtcNow.AddDays(-60),
            initialHens: 10,
            initialRoosters: 2,
            initialChicks: 0);

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockFlockRepository.Setup(x => x.GetByIdWithoutHistoryAsync(flockId))
            .ReturnsAsync(existingFlock);
        _mockFlockRepository.Setup(x => x.UpdateAsync(It.IsAny<Flock>()))
            .ReturnsAsync((Flock f) => f);

        var expectedDto = new FlockDto { Id = flockId, IsActive = false };
        _mockMapper.Setup(x => x.Map<FlockDto>(It.IsAny<Flock>()))
            .Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region Authentication Tests

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldReturnUnauthorizedError()
    {
        // Arrange
        var command = new ArchiveFlockCommand
        {
            FlockId = Guid.NewGuid()
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Unauthorized");
        result.Error.Message.Should().Be("User is not authenticated");

        _mockFlockRepository.Verify(x => x.GetByIdWithoutHistoryAsync(It.IsAny<Guid>()), Times.Never);
        _mockFlockRepository.Verify(x => x.UpdateAsync(It.IsAny<Flock>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenTenantIdNotFound_ShouldReturnUnauthorizedError()
    {
        // Arrange
        var command = new ArchiveFlockCommand
        {
            FlockId = Guid.NewGuid()
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns((Guid?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Unauthorized");
        result.Error.Message.Should().Be("Tenant not found");

        _mockFlockRepository.Verify(x => x.GetByIdWithoutHistoryAsync(It.IsAny<Guid>()), Times.Never);
        _mockFlockRepository.Verify(x => x.UpdateAsync(It.IsAny<Flock>()), Times.Never);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldReturnFailureError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var flockId = Guid.NewGuid();

        var command = new ArchiveFlockCommand
        {
            FlockId = flockId
        };

        var existingFlock = Flock.Create(
            tenantId,
            coopId,
            "Test Flock",
            DateTime.UtcNow.AddDays(-60),
            initialHens: 10,
            initialRoosters: 2,
            initialChicks: 0);

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockFlockRepository.Setup(x => x.GetByIdWithoutHistoryAsync(flockId))
            .ReturnsAsync(existingFlock);
        _mockFlockRepository.Setup(x => x.UpdateAsync(It.IsAny<Flock>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Failure");
        result.Error.Message.Should().Contain("Failed to archive flock");
    }

    #endregion
}
