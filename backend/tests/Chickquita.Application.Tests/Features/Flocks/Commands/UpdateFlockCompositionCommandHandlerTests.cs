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
/// Unit tests for UpdateFlockCompositionCommandHandler.
/// Tests cover happy path, flock not found, validation errors, and exception handling.
/// </summary>
public class UpdateFlockCompositionCommandHandlerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IFlockRepository> _mockFlockRepository;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<UpdateFlockCompositionCommandHandler>> _mockLogger;
    private readonly UpdateFlockCompositionCommandHandler _handler;

    public UpdateFlockCompositionCommandHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());

        _mockFlockRepository = _fixture.Freeze<Mock<IFlockRepository>>();
        _mockCurrentUserService = _fixture.Freeze<Mock<ICurrentUserService>>();
        _mockMapper = _fixture.Freeze<Mock<IMapper>>();
        _mockLogger = _fixture.Freeze<Mock<ILogger<UpdateFlockCompositionCommandHandler>>>();

        _handler = new UpdateFlockCompositionCommandHandler(
            _mockFlockRepository.Object,
            _mockCurrentUserService.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    #region Happy Path Tests

    [Fact]
    public async Task Handle_WithValidData_ShouldUpdateCompositionSuccessfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var flockId = Guid.NewGuid();

        var command = new UpdateFlockCompositionCommand
        {
            FlockId = flockId,
            Hens = 20,
            Roosters = 5,
            Chicks = 10
        };

        var existingFlock = Flock.Create(
            tenantId,
            coopId,
            "Test Flock",
            DateTime.UtcNow.AddDays(-60),
            initialHens: 10,
            initialRoosters: 2,
            initialChicks: 5).Value;

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockFlockRepository.Setup(x => x.GetByIdAsync(flockId))
            .ReturnsAsync(existingFlock);
        _mockFlockRepository.Setup(x => x.UpdateAsync(It.IsAny<Flock>()))
            .ReturnsAsync((Flock f) => f);

        var expectedDto = new FlockDto
        {
            Id = flockId,
            TenantId = tenantId,
            CoopId = coopId,
            CurrentHens = 20,
            CurrentRoosters = 5,
            CurrentChicks = 10,
            IsActive = true
        };

        _mockMapper.Setup(x => x.Map<FlockDto>(It.IsAny<Flock>()))
            .Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.CurrentHens.Should().Be(20);
        result.Value.CurrentRoosters.Should().Be(5);
        result.Value.CurrentChicks.Should().Be(10);

        _mockFlockRepository.Verify(x => x.GetByIdAsync(flockId), Times.Once);
        _mockFlockRepository.Verify(x => x.UpdateAsync(It.IsAny<Flock>()), Times.Once);
        _mockMapper.Verify(x => x.Map<FlockDto>(It.IsAny<Flock>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldCreateFlockHistoryEntry()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var flockId = Guid.NewGuid();

        var command = new UpdateFlockCompositionCommand
        {
            FlockId = flockId,
            Hens = 15,
            Roosters = 3,
            Chicks = 0,
            Notes = "Seasonal adjustment"
        };

        var existingFlock = Flock.Create(
            tenantId,
            coopId,
            "Test Flock",
            DateTime.UtcNow.AddDays(-30),
            initialHens: 10,
            initialRoosters: 2,
            initialChicks: 5).Value;

        Flock? capturedFlock = null;

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockFlockRepository.Setup(x => x.GetByIdAsync(flockId))
            .ReturnsAsync(existingFlock);
        _mockFlockRepository.Setup(x => x.UpdateAsync(It.IsAny<Flock>()))
            .Callback<Flock>(f => capturedFlock = f)
            .ReturnsAsync((Flock f) => f);

        _mockMapper.Setup(x => x.Map<FlockDto>(It.IsAny<Flock>()))
            .Returns(new FlockDto { Id = flockId });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedFlock.Should().NotBeNull();
        capturedFlock!.CurrentHens.Should().Be(15);
        capturedFlock.CurrentRoosters.Should().Be(3);
        capturedFlock.CurrentChicks.Should().Be(0);
        // History entry should have been created (1 initial from Create + 1 from UpdateComposition)
        capturedFlock.History.Should().HaveCountGreaterThan(1);
    }

    [Fact]
    public async Task Handle_WithZeroValues_ShouldUpdateSuccessfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var flockId = Guid.NewGuid();

        var command = new UpdateFlockCompositionCommand
        {
            FlockId = flockId,
            Hens = 0,
            Roosters = 0,
            Chicks = 0
        };

        var existingFlock = Flock.Create(
            tenantId,
            coopId,
            "Test Flock",
            DateTime.UtcNow.AddDays(-30),
            initialHens: 5,
            initialRoosters: 1,
            initialChicks: 2).Value;

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockFlockRepository.Setup(x => x.GetByIdAsync(flockId))
            .ReturnsAsync(existingFlock);
        _mockFlockRepository.Setup(x => x.UpdateAsync(It.IsAny<Flock>()))
            .ReturnsAsync((Flock f) => f);

        _mockMapper.Setup(x => x.Map<FlockDto>(It.IsAny<Flock>()))
            .Returns(new FlockDto { Id = flockId });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region Flock Not Found Tests

    [Fact]
    public async Task Handle_WhenFlockDoesNotExist_ShouldReturnNotFoundError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var flockId = Guid.NewGuid();

        var command = new UpdateFlockCompositionCommand
        {
            FlockId = flockId,
            Hens = 10,
            Roosters = 2,
            Chicks = 5
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockFlockRepository.Setup(x => x.GetByIdAsync(flockId))
            .ReturnsAsync((Flock?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.NotFound");
        result.Error.Message.Should().Be("Flock not found");

        _mockFlockRepository.Verify(x => x.GetByIdAsync(flockId), Times.Once);
        _mockFlockRepository.Verify(x => x.UpdateAsync(It.IsAny<Flock>()), Times.Never);
    }

    #endregion

    #region Validation Error Tests

    [Fact]
    public async Task Handle_WithNegativeHens_ShouldReturnValidationError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var flockId = Guid.NewGuid();

        var command = new UpdateFlockCompositionCommand
        {
            FlockId = flockId,
            Hens = -1,
            Roosters = 2,
            Chicks = 0
        };

        var existingFlock = Flock.Create(
            tenantId,
            coopId,
            "Test Flock",
            DateTime.UtcNow.AddDays(-30),
            initialHens: 5,
            initialRoosters: 1,
            initialChicks: 0).Value;

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockFlockRepository.Setup(x => x.GetByIdAsync(flockId))
            .ReturnsAsync(existingFlock);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Validation");

        _mockFlockRepository.Verify(x => x.UpdateAsync(It.IsAny<Flock>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNegativeRoosters_ShouldReturnValidationError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var flockId = Guid.NewGuid();

        var command = new UpdateFlockCompositionCommand
        {
            FlockId = flockId,
            Hens = 5,
            Roosters = -1,
            Chicks = 0
        };

        var existingFlock = Flock.Create(
            tenantId,
            coopId,
            "Test Flock",
            DateTime.UtcNow.AddDays(-30),
            initialHens: 5,
            initialRoosters: 1,
            initialChicks: 0).Value;

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockFlockRepository.Setup(x => x.GetByIdAsync(flockId))
            .ReturnsAsync(existingFlock);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Validation");

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

        var command = new UpdateFlockCompositionCommand
        {
            FlockId = flockId,
            Hens = 10,
            Roosters = 2,
            Chicks = 5
        };

        var existingFlock = Flock.Create(
            tenantId,
            coopId,
            "Test Flock",
            DateTime.UtcNow.AddDays(-30),
            initialHens: 5,
            initialRoosters: 1,
            initialChicks: 2).Value;

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockFlockRepository.Setup(x => x.GetByIdAsync(flockId))
            .ReturnsAsync(existingFlock);
        _mockFlockRepository.Setup(x => x.UpdateAsync(It.IsAny<Flock>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Failure");
        result.Error.Message.Should().Be("An unexpected error occurred. Please try again.");
    }

    #endregion
}
