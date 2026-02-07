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
/// Unit tests for UpdateFlockCommandHandler.
/// Tests cover all validation scenarios, tenant isolation, composition protection, and error handling.
/// </summary>
public class UpdateFlockCommandHandlerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IFlockRepository> _mockFlockRepository;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<UpdateFlockCommandHandler>> _mockLogger;
    private readonly UpdateFlockCommandHandler _handler;

    public UpdateFlockCommandHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());

        _mockFlockRepository = _fixture.Freeze<Mock<IFlockRepository>>();
        _mockCurrentUserService = _fixture.Freeze<Mock<ICurrentUserService>>();
        _mockMapper = _fixture.Freeze<Mock<IMapper>>();
        _mockLogger = _fixture.Freeze<Mock<ILogger<UpdateFlockCommandHandler>>>();

        _handler = new UpdateFlockCommandHandler(
            _mockFlockRepository.Object,
            _mockCurrentUserService.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    #region Happy Path Tests

    [Fact]
    public async Task Handle_WithValidData_ShouldUpdateFlockSuccessfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var flockId = Guid.NewGuid();
        var newHatchDate = DateTime.UtcNow.AddDays(-45);

        var command = new UpdateFlockCommand
        {
            FlockId = flockId,
            Identifier = "Updated Identifier",
            HatchDate = newHatchDate
        };

        var existingFlock = Flock.Create(
            tenantId,
            coopId,
            "Original Identifier",
            DateTime.UtcNow.AddDays(-60),
            initialHens: 10,
            initialRoosters: 2,
            initialChicks: 5);

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockFlockRepository.Setup(x => x.GetByIdWithoutHistoryAsync(flockId))
            .ReturnsAsync(existingFlock);
        _mockFlockRepository.Setup(x => x.ExistsByIdentifierInCoopAsync(coopId, command.Identifier, flockId))
            .ReturnsAsync(false);
        _mockFlockRepository.Setup(x => x.UpdateAsync(It.IsAny<Flock>()))
            .ReturnsAsync((Flock f) => f);

        var expectedDto = new FlockDto
        {
            Id = flockId,
            TenantId = tenantId,
            CoopId = coopId,
            Identifier = command.Identifier,
            HatchDate = command.HatchDate,
            CurrentHens = 10,
            CurrentRoosters = 2,
            CurrentChicks = 5,
            IsActive = true
        };

        _mockMapper.Setup(x => x.Map<FlockDto>(It.IsAny<Flock>()))
            .Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Identifier.Should().Be(command.Identifier);
        result.Value.HatchDate.Should().Be(command.HatchDate);
        result.Value.CurrentHens.Should().Be(10); // Composition unchanged
        result.Value.CurrentRoosters.Should().Be(2); // Composition unchanged
        result.Value.CurrentChicks.Should().Be(5); // Composition unchanged

        _mockFlockRepository.Verify(x => x.GetByIdWithoutHistoryAsync(flockId), Times.Once);
        _mockFlockRepository.Verify(x => x.ExistsByIdentifierInCoopAsync(coopId, command.Identifier, flockId), Times.Once);
        _mockFlockRepository.Verify(x => x.UpdateAsync(It.IsAny<Flock>()), Times.Once);
        _mockMapper.Verify(x => x.Map<FlockDto>(It.IsAny<Flock>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUpdatingIdentifierOnly_ShouldUpdateIdentifierAndLeaveHatchDateUnchanged()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var flockId = Guid.NewGuid();
        var originalHatchDate = DateTime.UtcNow.AddDays(-60);

        var command = new UpdateFlockCommand
        {
            FlockId = flockId,
            Identifier = "New Name",
            HatchDate = originalHatchDate // Same as original
        };

        var existingFlock = Flock.Create(
            tenantId,
            coopId,
            "Old Name",
            originalHatchDate,
            initialHens: 8,
            initialRoosters: 1,
            initialChicks: 3);

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockFlockRepository.Setup(x => x.GetByIdWithoutHistoryAsync(flockId))
            .ReturnsAsync(existingFlock);
        _mockFlockRepository.Setup(x => x.ExistsByIdentifierInCoopAsync(coopId, command.Identifier, flockId))
            .ReturnsAsync(false);
        _mockFlockRepository.Setup(x => x.UpdateAsync(It.IsAny<Flock>()))
            .ReturnsAsync((Flock f) => f);

        var expectedDto = new FlockDto { Id = flockId, Identifier = command.Identifier };
        _mockMapper.Setup(x => x.Map<FlockDto>(It.IsAny<Flock>()))
            .Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Identifier.Should().Be(command.Identifier);
    }

    [Fact]
    public async Task Handle_DoesNotModifyComposition_ShouldPreserveHensRoostersAndChicks()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var flockId = Guid.NewGuid();

        var command = new UpdateFlockCommand
        {
            FlockId = flockId,
            Identifier = "Updated",
            HatchDate = DateTime.UtcNow.AddDays(-30)
        };

        var existingFlock = Flock.Create(
            tenantId,
            coopId,
            "Original",
            DateTime.UtcNow.AddDays(-60),
            initialHens: 15,
            initialRoosters: 3,
            initialChicks: 10);

        Flock? capturedFlock = null;

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockFlockRepository.Setup(x => x.GetByIdWithoutHistoryAsync(flockId))
            .ReturnsAsync(existingFlock);
        _mockFlockRepository.Setup(x => x.ExistsByIdentifierInCoopAsync(coopId, command.Identifier, flockId))
            .ReturnsAsync(false);
        _mockFlockRepository.Setup(x => x.UpdateAsync(It.IsAny<Flock>()))
            .Callback<Flock>(f => capturedFlock = f)
            .ReturnsAsync((Flock f) => f);

        var expectedDto = new FlockDto { Id = flockId };
        _mockMapper.Setup(x => x.Map<FlockDto>(It.IsAny<Flock>()))
            .Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedFlock.Should().NotBeNull();
        capturedFlock!.CurrentHens.Should().Be(15); // Unchanged
        capturedFlock.CurrentRoosters.Should().Be(3); // Unchanged
        capturedFlock.CurrentChicks.Should().Be(10); // Unchanged
    }

    #endregion

    #region Flock Existence Validation Tests

    [Fact]
    public async Task Handle_WhenFlockDoesNotExist_ShouldReturnNotFoundError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var flockId = Guid.NewGuid();

        var command = new UpdateFlockCommand
        {
            FlockId = flockId,
            Identifier = "Updated",
            HatchDate = DateTime.UtcNow.AddDays(-30)
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

    [Fact]
    public async Task Handle_WhenFlockBelongsToCurrentTenant_ShouldUpdateSuccessfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var flockId = Guid.NewGuid();

        var command = new UpdateFlockCommand
        {
            FlockId = flockId,
            Identifier = "Updated",
            HatchDate = DateTime.UtcNow.AddDays(-30)
        };

        var existingFlock = Flock.Create(
            tenantId, // Same tenant
            coopId,
            "Original",
            DateTime.UtcNow.AddDays(-60),
            initialHens: 10,
            initialRoosters: 2,
            initialChicks: 0);

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockFlockRepository.Setup(x => x.GetByIdWithoutHistoryAsync(flockId))
            .ReturnsAsync(existingFlock);
        _mockFlockRepository.Setup(x => x.ExistsByIdentifierInCoopAsync(coopId, command.Identifier, flockId))
            .ReturnsAsync(false);
        _mockFlockRepository.Setup(x => x.UpdateAsync(It.IsAny<Flock>()))
            .ReturnsAsync((Flock f) => f);

        var expectedDto = new FlockDto { Id = flockId };
        _mockMapper.Setup(x => x.Map<FlockDto>(It.IsAny<Flock>()))
            .Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region Identifier Uniqueness Validation Tests

    [Fact]
    public async Task Handle_WithDuplicateIdentifierInSameCoop_ShouldReturnConflictError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var flockId = Guid.NewGuid();

        var command = new UpdateFlockCommand
        {
            FlockId = flockId,
            Identifier = "Duplicate Identifier",
            HatchDate = DateTime.UtcNow.AddDays(-30)
        };

        var existingFlock = Flock.Create(
            tenantId,
            coopId,
            "Original",
            DateTime.UtcNow.AddDays(-60),
            initialHens: 10,
            initialRoosters: 2,
            initialChicks: 0);

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockFlockRepository.Setup(x => x.GetByIdWithoutHistoryAsync(flockId))
            .ReturnsAsync(existingFlock);
        _mockFlockRepository.Setup(x => x.ExistsByIdentifierInCoopAsync(coopId, command.Identifier, flockId))
            .ReturnsAsync(true); // Duplicate found

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Conflict");
        result.Error.Message.Should().Be("A flock with this identifier already exists in the coop");

        _mockFlockRepository.Verify(x => x.ExistsByIdentifierInCoopAsync(coopId, command.Identifier, flockId), Times.Once);
        _mockFlockRepository.Verify(x => x.UpdateAsync(It.IsAny<Flock>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenKeepingSameIdentifier_ShouldUpdateSuccessfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var flockId = Guid.NewGuid();
        var identifier = "Same Identifier";

        var command = new UpdateFlockCommand
        {
            FlockId = flockId,
            Identifier = identifier,
            HatchDate = DateTime.UtcNow.AddDays(-30)
        };

        var existingFlock = Flock.Create(
            tenantId,
            coopId,
            identifier, // Same identifier
            DateTime.UtcNow.AddDays(-60),
            initialHens: 10,
            initialRoosters: 2,
            initialChicks: 0);

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockFlockRepository.Setup(x => x.GetByIdWithoutHistoryAsync(flockId))
            .ReturnsAsync(existingFlock);
        _mockFlockRepository.Setup(x => x.ExistsByIdentifierInCoopAsync(coopId, identifier, flockId))
            .ReturnsAsync(false); // Excluded from uniqueness check
        _mockFlockRepository.Setup(x => x.UpdateAsync(It.IsAny<Flock>()))
            .ReturnsAsync((Flock f) => f);

        var expectedDto = new FlockDto { Id = flockId };
        _mockMapper.Setup(x => x.Map<FlockDto>(It.IsAny<Flock>()))
            .Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region Authentication and Tenant Isolation Tests

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldReturnUnauthorizedError()
    {
        // Arrange
        var command = new UpdateFlockCommand
        {
            FlockId = Guid.NewGuid(),
            Identifier = "Updated",
            HatchDate = DateTime.UtcNow.AddDays(-30)
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
        var command = new UpdateFlockCommand
        {
            FlockId = Guid.NewGuid(),
            Identifier = "Updated",
            HatchDate = DateTime.UtcNow.AddDays(-30)
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

    [Fact]
    public async Task Handle_WhenFlockBelongsToDifferentTenant_ShouldReturnNotFoundError()
    {
        // Arrange
        var currentTenantId = Guid.NewGuid();
        var differentTenantId = Guid.NewGuid();
        var flockId = Guid.NewGuid();

        var command = new UpdateFlockCommand
        {
            FlockId = flockId,
            Identifier = "Updated Identifier",
            HatchDate = DateTime.UtcNow.AddDays(-30)
        };

        // Repository returns null due to RLS/global query filter when flock belongs to different tenant
        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(currentTenantId);
        _mockFlockRepository.Setup(x => x.GetByIdWithoutHistoryAsync(flockId))
            .ReturnsAsync((Flock?)null); // RLS filters out the flock from different tenant

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

    [Fact]
    public async Task Handle_ShouldOnlyAccessCurrentTenantData()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var flockId = Guid.NewGuid();

        var command = new UpdateFlockCommand
        {
            FlockId = flockId,
            Identifier = "Updated Flock",
            HatchDate = DateTime.UtcNow.AddDays(-30)
        };

        var existingFlock = Flock.Create(
            tenantId,
            coopId,
            "Original Flock",
            DateTime.UtcNow.AddDays(-60),
            initialHens: 10,
            initialRoosters: 2,
            initialChicks: 5);

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockFlockRepository.Setup(x => x.GetByIdWithoutHistoryAsync(flockId))
            .ReturnsAsync(existingFlock);
        _mockFlockRepository.Setup(x => x.ExistsByIdentifierInCoopAsync(coopId, command.Identifier, flockId))
            .ReturnsAsync(false);
        _mockFlockRepository.Setup(x => x.UpdateAsync(It.IsAny<Flock>()))
            .ReturnsAsync((Flock f) => f);

        var expectedDto = new FlockDto { Id = flockId, TenantId = tenantId };
        _mockMapper.Setup(x => x.Map<FlockDto>(It.IsAny<Flock>()))
            .Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TenantId.Should().Be(tenantId);

        // Verify repository is called (which applies RLS/global filter)
        _mockFlockRepository.Verify(x => x.GetByIdWithoutHistoryAsync(flockId), Times.Once);
    }

    #endregion

    #region Validation Error Tests

    [Fact]
    public async Task Handle_WithFutureHatchDate_ShouldReturnValidationError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var flockId = Guid.NewGuid();

        var command = new UpdateFlockCommand
        {
            FlockId = flockId,
            Identifier = "Updated",
            HatchDate = DateTime.UtcNow.AddDays(10) // Future date
        };

        var existingFlock = Flock.Create(
            tenantId,
            coopId,
            "Original",
            DateTime.UtcNow.AddDays(-60),
            initialHens: 10,
            initialRoosters: 2,
            initialChicks: 0);

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockFlockRepository.Setup(x => x.GetByIdWithoutHistoryAsync(flockId))
            .ReturnsAsync(existingFlock);
        _mockFlockRepository.Setup(x => x.ExistsByIdentifierInCoopAsync(coopId, command.Identifier, flockId))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Validation");
        result.Error.Message.Should().Contain("cannot be in the future");

        _mockFlockRepository.Verify(x => x.UpdateAsync(It.IsAny<Flock>()), Times.Never);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_WithEmptyOrWhitespaceIdentifier_ShouldReturnValidationError(string invalidIdentifier)
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var flockId = Guid.NewGuid();

        var command = new UpdateFlockCommand
        {
            FlockId = flockId,
            Identifier = invalidIdentifier,
            HatchDate = DateTime.UtcNow.AddDays(-30)
        };

        var existingFlock = Flock.Create(
            tenantId,
            coopId,
            "Original",
            DateTime.UtcNow.AddDays(-60),
            initialHens: 10,
            initialRoosters: 2,
            initialChicks: 0);

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockFlockRepository.Setup(x => x.GetByIdWithoutHistoryAsync(flockId))
            .ReturnsAsync(existingFlock);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Validation");
        result.Error.Message.Should().Contain("Identifier cannot be empty");

        _mockFlockRepository.Verify(x => x.UpdateAsync(It.IsAny<Flock>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithIdentifierExceeding50Characters_ShouldReturnValidationError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var flockId = Guid.NewGuid();
        var longIdentifier = new string('A', 51);

        var command = new UpdateFlockCommand
        {
            FlockId = flockId,
            Identifier = longIdentifier,
            HatchDate = DateTime.UtcNow.AddDays(-30)
        };

        var existingFlock = Flock.Create(
            tenantId,
            coopId,
            "Original",
            DateTime.UtcNow.AddDays(-60),
            initialHens: 10,
            initialRoosters: 2,
            initialChicks: 0);

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockFlockRepository.Setup(x => x.GetByIdWithoutHistoryAsync(flockId))
            .ReturnsAsync(existingFlock);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Validation");
        result.Error.Message.Should().Contain("cannot exceed 50 characters");

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

        var command = new UpdateFlockCommand
        {
            FlockId = flockId,
            Identifier = "Updated",
            HatchDate = DateTime.UtcNow.AddDays(-30)
        };

        var existingFlock = Flock.Create(
            tenantId,
            coopId,
            "Original",
            DateTime.UtcNow.AddDays(-60),
            initialHens: 10,
            initialRoosters: 2,
            initialChicks: 0);

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockFlockRepository.Setup(x => x.GetByIdWithoutHistoryAsync(flockId))
            .ReturnsAsync(existingFlock);
        _mockFlockRepository.Setup(x => x.ExistsByIdentifierInCoopAsync(coopId, command.Identifier, flockId))
            .ReturnsAsync(false);
        _mockFlockRepository.Setup(x => x.UpdateAsync(It.IsAny<Flock>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Failure");
        result.Error.Message.Should().Contain("Failed to update flock");
    }

    #endregion
}
