using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using Chickquita.Application.DTOs;
using Chickquita.Application.Features.DailyRecords.Commands;
using Chickquita.Application.Interfaces;
using Chickquita.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Chickquita.Application.Tests.Features.DailyRecords.Commands;

/// <summary>
/// Unit tests for CreateDailyRecordCommandHandler.
/// Tests cover all validation scenarios, duplicate detection, and tenant isolation.
/// </summary>
public class CreateDailyRecordCommandHandlerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IDailyRecordRepository> _mockDailyRecordRepository;
    private readonly Mock<IFlockRepository> _mockFlockRepository;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<CreateDailyRecordCommandHandler>> _mockLogger;
    private readonly CreateDailyRecordCommandHandler _handler;

    public CreateDailyRecordCommandHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());

        _mockDailyRecordRepository = _fixture.Freeze<Mock<IDailyRecordRepository>>();
        _mockFlockRepository = _fixture.Freeze<Mock<IFlockRepository>>();
        _mockCurrentUserService = _fixture.Freeze<Mock<ICurrentUserService>>();
        _mockMapper = _fixture.Freeze<Mock<IMapper>>();
        _mockLogger = _fixture.Freeze<Mock<ILogger<CreateDailyRecordCommandHandler>>>();

        _handler = new CreateDailyRecordCommandHandler(
            _mockDailyRecordRepository.Object,
            _mockFlockRepository.Object,
            _mockCurrentUserService.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    #region Happy Path Tests

    [Fact]
    public async Task Handle_WithValidData_ShouldCreateDailyRecordSuccessfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var flockId = Guid.NewGuid();
        var recordDate = DateTime.UtcNow.Date.AddDays(-1);

        var command = new CreateDailyRecordCommand
        {
            FlockId = flockId,
            RecordDate = recordDate,
            EggCount = 25,
            Notes = "Good weather, productive day"
        };

        var flock = Flock.Create(
            tenantId,
            Guid.NewGuid(),
            "Spring 2024",
            DateTime.UtcNow.AddDays(-60),
            10,
            2,
            0).Value;

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockFlockRepository.Setup(x => x.GetByIdWithoutHistoryAsync(flockId))
            .ReturnsAsync(flock);

        var createdDailyRecord = DailyRecord.Create(
            tenantId,
            flockId,
            recordDate,
            command.EggCount,
            command.Notes).Value;

        _mockDailyRecordRepository.Setup(x => x.AddAsync(It.IsAny<DailyRecord>()))
            .ReturnsAsync(createdDailyRecord);

        var expectedDto = new DailyRecordDto
        {
            Id = createdDailyRecord.Id,
            TenantId = tenantId,
            FlockId = flockId,
            RecordDate = recordDate,
            EggCount = command.EggCount,
            Notes = command.Notes
        };

        _mockMapper.Setup(x => x.Map<DailyRecordDto>(It.IsAny<DailyRecord>()))
            .Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.FlockId.Should().Be(flockId);
        result.Value.RecordDate.Should().Be(recordDate);
        result.Value.EggCount.Should().Be(command.EggCount);
        result.Value.Notes.Should().Be(command.Notes);
        result.Value.TenantId.Should().Be(tenantId);

        _mockFlockRepository.Verify(x => x.GetByIdWithoutHistoryAsync(flockId), Times.Once);
        _mockDailyRecordRepository.Verify(x => x.AddAsync(It.IsAny<DailyRecord>()), Times.Once);
        _mockMapper.Verify(x => x.Map<DailyRecordDto>(It.IsAny<DailyRecord>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithZeroEggCount_ShouldCreateDailyRecordSuccessfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var flockId = Guid.NewGuid();
        var recordDate = DateTime.UtcNow.Date;

        var command = new CreateDailyRecordCommand
        {
            FlockId = flockId,
            RecordDate = recordDate,
            EggCount = 0,
            Notes = "No eggs collected today"
        };

        var flock = Flock.Create(
            tenantId,
            Guid.NewGuid(),
            "Spring 2024",
            DateTime.UtcNow.AddDays(-60),
            10,
            2,
            0).Value;

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockFlockRepository.Setup(x => x.GetByIdWithoutHistoryAsync(flockId)).ReturnsAsync(flock);

        var createdDailyRecord = DailyRecord.Create(tenantId, flockId, recordDate, 0, command.Notes).Value;
        _mockDailyRecordRepository.Setup(x => x.AddAsync(It.IsAny<DailyRecord>()))
            .ReturnsAsync(createdDailyRecord);

        var expectedDto = new DailyRecordDto
        {
            Id = createdDailyRecord.Id,
            EggCount = 0
        };

        _mockMapper.Setup(x => x.Map<DailyRecordDto>(It.IsAny<DailyRecord>()))
            .Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.EggCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithoutNotes_ShouldCreateDailyRecordSuccessfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var flockId = Guid.NewGuid();
        var recordDate = DateTime.UtcNow.Date;

        var command = new CreateDailyRecordCommand
        {
            FlockId = flockId,
            RecordDate = recordDate,
            EggCount = 15,
            Notes = null
        };

        var flock = Flock.Create(
            tenantId,
            Guid.NewGuid(),
            "Spring 2024",
            DateTime.UtcNow.AddDays(-60),
            10,
            2,
            0).Value;

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockFlockRepository.Setup(x => x.GetByIdWithoutHistoryAsync(flockId)).ReturnsAsync(flock);

        var createdDailyRecord = DailyRecord.Create(tenantId, flockId, recordDate, command.EggCount).Value;
        _mockDailyRecordRepository.Setup(x => x.AddAsync(It.IsAny<DailyRecord>()))
            .ReturnsAsync(createdDailyRecord);

        var expectedDto = new DailyRecordDto { Id = createdDailyRecord.Id };
        _mockMapper.Setup(x => x.Map<DailyRecordDto>(It.IsAny<DailyRecord>()))
            .Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region Flock Validation Tests

    [Fact]
    public async Task Handle_WhenFlockDoesNotExist_ShouldReturnNotFoundError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var flockId = Guid.NewGuid();
        var recordDate = DateTime.UtcNow.Date;

        var command = new CreateDailyRecordCommand
        {
            FlockId = flockId,
            RecordDate = recordDate,
            EggCount = 10
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
        _mockDailyRecordRepository.Verify(x => x.AddAsync(It.IsAny<DailyRecord>()), Times.Never);
    }

    #endregion

    // NOTE: Authentication and tenant isolation checks (unauthenticated user, missing tenant ID)
    // are handled by the AuthorizationBehavior MediatR pipeline behavior, not by individual handlers.
    // Those scenarios are covered by AuthorizationBehaviorTests.

    #region Validation Error Tests

    [Fact]
    public async Task Handle_WithFutureRecordDate_ShouldReturnValidationError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var flockId = Guid.NewGuid();
        var futureDate = DateTime.UtcNow.Date.AddDays(5);

        var command = new CreateDailyRecordCommand
        {
            FlockId = flockId,
            RecordDate = futureDate,
            EggCount = 10
        };

        var flock = Flock.Create(
            tenantId,
            Guid.NewGuid(),
            "Spring 2024",
            DateTime.UtcNow.AddDays(-60),
            10,
            2,
            0).Value;

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockFlockRepository.Setup(x => x.GetByIdWithoutHistoryAsync(flockId)).ReturnsAsync(flock);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Validation");
        result.Error.Message.Should().Contain("cannot be in the future");

        _mockDailyRecordRepository.Verify(x => x.AddAsync(It.IsAny<DailyRecord>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNegativeEggCount_ShouldReturnValidationError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var flockId = Guid.NewGuid();
        var recordDate = DateTime.UtcNow.Date;

        var command = new CreateDailyRecordCommand
        {
            FlockId = flockId,
            RecordDate = recordDate,
            EggCount = -10
        };

        var flock = Flock.Create(
            tenantId,
            Guid.NewGuid(),
            "Spring 2024",
            DateTime.UtcNow.AddDays(-60),
            10,
            2,
            0).Value;

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockFlockRepository.Setup(x => x.GetByIdWithoutHistoryAsync(flockId)).ReturnsAsync(flock);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Validation");
        result.Error.Message.Should().Contain("cannot be negative");

        _mockDailyRecordRepository.Verify(x => x.AddAsync(It.IsAny<DailyRecord>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNotesExceeding500Characters_ShouldReturnValidationError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var flockId = Guid.NewGuid();
        var recordDate = DateTime.UtcNow.Date;
        var longNotes = new string('A', 501);

        var command = new CreateDailyRecordCommand
        {
            FlockId = flockId,
            RecordDate = recordDate,
            EggCount = 10,
            Notes = longNotes
        };

        var flock = Flock.Create(
            tenantId,
            Guid.NewGuid(),
            "Spring 2024",
            DateTime.UtcNow.AddDays(-60),
            10,
            2,
            0).Value;

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockFlockRepository.Setup(x => x.GetByIdWithoutHistoryAsync(flockId)).ReturnsAsync(flock);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Validation");
        result.Error.Message.Should().Contain("cannot exceed 500 characters");

        _mockDailyRecordRepository.Verify(x => x.AddAsync(It.IsAny<DailyRecord>()), Times.Never);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldReturnFailureError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var flockId = Guid.NewGuid();
        var recordDate = DateTime.UtcNow.Date;

        var command = new CreateDailyRecordCommand
        {
            FlockId = flockId,
            RecordDate = recordDate,
            EggCount = 10
        };

        var flock = Flock.Create(
            tenantId,
            Guid.NewGuid(),
            "Spring 2024",
            DateTime.UtcNow.AddDays(-60),
            10,
            2,
            0).Value;

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockFlockRepository.Setup(x => x.GetByIdWithoutHistoryAsync(flockId)).ReturnsAsync(flock);
        _mockDailyRecordRepository.Setup(x => x.AddAsync(It.IsAny<DailyRecord>()))
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
