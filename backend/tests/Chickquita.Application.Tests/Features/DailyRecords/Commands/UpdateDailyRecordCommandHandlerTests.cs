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
/// Unit tests for UpdateDailyRecordCommandHandler.
/// Tests cover same-day edit restriction, validation scenarios, and tenant isolation.
/// </summary>
public class UpdateDailyRecordCommandHandlerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IDailyRecordRepository> _mockDailyRecordRepository;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<UpdateDailyRecordCommandHandler>> _mockLogger;
    private readonly UpdateDailyRecordCommandHandler _handler;

    public UpdateDailyRecordCommandHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());

        _mockDailyRecordRepository = _fixture.Freeze<Mock<IDailyRecordRepository>>();
        _mockCurrentUserService = _fixture.Freeze<Mock<ICurrentUserService>>();
        _mockMapper = _fixture.Freeze<Mock<IMapper>>();
        _mockLogger = _fixture.Freeze<Mock<ILogger<UpdateDailyRecordCommandHandler>>>();

        _handler = new UpdateDailyRecordCommandHandler(
            _mockDailyRecordRepository.Object,
            _mockCurrentUserService.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    #region Happy Path Tests

    [Fact]
    public async Task Handle_WithValidDataOnSameDay_ShouldUpdateDailyRecordSuccessfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var dailyRecordId = Guid.NewGuid();
        var flockId = Guid.NewGuid();
        var today = DateTime.UtcNow.Date;

        var command = new UpdateDailyRecordCommand
        {
            Id = dailyRecordId,
            EggCount = 30,
            Notes = "Updated: Great weather, very productive"
        };

        // Create a daily record with today's date
        var existingDailyRecord = DailyRecord.Create(
            tenantId,
            flockId,
            today,
            25,
            "Original notes");

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockDailyRecordRepository.Setup(x => x.GetByIdWithoutNavigationAsync(dailyRecordId))
            .ReturnsAsync(existingDailyRecord);
        _mockDailyRecordRepository.Setup(x => x.UpdateAsync(It.IsAny<DailyRecord>()))
            .ReturnsAsync(existingDailyRecord);

        var expectedDto = new DailyRecordDto
        {
            Id = dailyRecordId,
            TenantId = tenantId,
            FlockId = flockId,
            RecordDate = today,
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
        result.Value.EggCount.Should().Be(command.EggCount);
        result.Value.Notes.Should().Be(command.Notes);

        _mockDailyRecordRepository.Verify(x => x.GetByIdWithoutNavigationAsync(dailyRecordId), Times.Once);
        _mockDailyRecordRepository.Verify(x => x.UpdateAsync(It.IsAny<DailyRecord>()), Times.Once);
        _mockMapper.Verify(x => x.Map<DailyRecordDto>(It.IsAny<DailyRecord>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithZeroEggCountOnSameDay_ShouldUpdateSuccessfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var dailyRecordId = Guid.NewGuid();
        var flockId = Guid.NewGuid();
        var today = DateTime.UtcNow.Date;

        var command = new UpdateDailyRecordCommand
        {
            Id = dailyRecordId,
            EggCount = 0,
            Notes = "No eggs today"
        };

        var existingDailyRecord = DailyRecord.Create(tenantId, flockId, today, 5, "Original");

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockDailyRecordRepository.Setup(x => x.GetByIdWithoutNavigationAsync(dailyRecordId))
            .ReturnsAsync(existingDailyRecord);
        _mockDailyRecordRepository.Setup(x => x.UpdateAsync(It.IsAny<DailyRecord>()))
            .ReturnsAsync(existingDailyRecord);

        var expectedDto = new DailyRecordDto { Id = dailyRecordId, EggCount = 0 };
        _mockMapper.Setup(x => x.Map<DailyRecordDto>(It.IsAny<DailyRecord>()))
            .Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.EggCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithNullNotesOnSameDay_ShouldUpdateSuccessfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var dailyRecordId = Guid.NewGuid();
        var flockId = Guid.NewGuid();
        var today = DateTime.UtcNow.Date;

        var command = new UpdateDailyRecordCommand
        {
            Id = dailyRecordId,
            EggCount = 20,
            Notes = null
        };

        var existingDailyRecord = DailyRecord.Create(tenantId, flockId, today, 15, "Old notes");

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockDailyRecordRepository.Setup(x => x.GetByIdWithoutNavigationAsync(dailyRecordId))
            .ReturnsAsync(existingDailyRecord);
        _mockDailyRecordRepository.Setup(x => x.UpdateAsync(It.IsAny<DailyRecord>()))
            .ReturnsAsync(existingDailyRecord);

        var expectedDto = new DailyRecordDto { Id = dailyRecordId, Notes = null };
        _mockMapper.Setup(x => x.Map<DailyRecordDto>(It.IsAny<DailyRecord>()))
            .Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region Same-Day Edit Restriction Tests

    [Fact]
    public async Task Handle_WhenRecordDateIsYesterday_ShouldReturnValidationError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var dailyRecordId = Guid.NewGuid();
        var flockId = Guid.NewGuid();
        var yesterday = DateTime.UtcNow.Date.AddDays(-1);

        var command = new UpdateDailyRecordCommand
        {
            Id = dailyRecordId,
            EggCount = 30,
            Notes = "Trying to update yesterday's record"
        };

        // Create a daily record from yesterday
        var existingDailyRecord = DailyRecord.Create(tenantId, flockId, yesterday, 25, "Original");

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockDailyRecordRepository.Setup(x => x.GetByIdWithoutNavigationAsync(dailyRecordId))
            .ReturnsAsync(existingDailyRecord);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Validation");
        result.Error.Message.Should().Contain("same-day edit restriction");

        _mockDailyRecordRepository.Verify(x => x.GetByIdWithoutNavigationAsync(dailyRecordId), Times.Once);
        _mockDailyRecordRepository.Verify(x => x.UpdateAsync(It.IsAny<DailyRecord>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRecordDateIsOneWeekAgo_ShouldReturnValidationError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var dailyRecordId = Guid.NewGuid();
        var flockId = Guid.NewGuid();
        var oneWeekAgo = DateTime.UtcNow.Date.AddDays(-7);

        var command = new UpdateDailyRecordCommand
        {
            Id = dailyRecordId,
            EggCount = 35,
            Notes = "Trying to update old record"
        };

        var existingDailyRecord = DailyRecord.Create(tenantId, flockId, oneWeekAgo, 20, "Old data");

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockDailyRecordRepository.Setup(x => x.GetByIdWithoutNavigationAsync(dailyRecordId))
            .ReturnsAsync(existingDailyRecord);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Validation");
        result.Error.Message.Should().Contain("same-day edit restriction");

        _mockDailyRecordRepository.Verify(x => x.UpdateAsync(It.IsAny<DailyRecord>()), Times.Never);
    }

    #endregion

    #region Daily Record Not Found Tests

    [Fact]
    public async Task Handle_WhenDailyRecordDoesNotExist_ShouldReturnNotFoundError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var dailyRecordId = Guid.NewGuid();

        var command = new UpdateDailyRecordCommand
        {
            Id = dailyRecordId,
            EggCount = 20
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockDailyRecordRepository.Setup(x => x.GetByIdWithoutNavigationAsync(dailyRecordId))
            .ReturnsAsync((DailyRecord?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.NotFound");
        result.Error.Message.Should().Be("Daily record not found");

        _mockDailyRecordRepository.Verify(x => x.GetByIdWithoutNavigationAsync(dailyRecordId), Times.Once);
        _mockDailyRecordRepository.Verify(x => x.UpdateAsync(It.IsAny<DailyRecord>()), Times.Never);
    }

    #endregion

    #region Authentication and Tenant Isolation Tests

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldReturnUnauthorizedError()
    {
        // Arrange
        var command = new UpdateDailyRecordCommand
        {
            Id = Guid.NewGuid(),
            EggCount = 20
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Unauthorized");
        result.Error.Message.Should().Be("User is not authenticated");

        _mockDailyRecordRepository.Verify(x => x.GetByIdWithoutNavigationAsync(It.IsAny<Guid>()), Times.Never);
        _mockDailyRecordRepository.Verify(x => x.UpdateAsync(It.IsAny<DailyRecord>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenTenantIdNotFound_ShouldReturnUnauthorizedError()
    {
        // Arrange
        var command = new UpdateDailyRecordCommand
        {
            Id = Guid.NewGuid(),
            EggCount = 20
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

        _mockDailyRecordRepository.Verify(x => x.GetByIdWithoutNavigationAsync(It.IsAny<Guid>()), Times.Never);
        _mockDailyRecordRepository.Verify(x => x.UpdateAsync(It.IsAny<DailyRecord>()), Times.Never);
    }

    #endregion

    #region Validation Error Tests

    [Fact]
    public async Task Handle_WithNegativeEggCount_ShouldReturnValidationError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var dailyRecordId = Guid.NewGuid();
        var flockId = Guid.NewGuid();
        var today = DateTime.UtcNow.Date;

        var command = new UpdateDailyRecordCommand
        {
            Id = dailyRecordId,
            EggCount = -5,
            Notes = "Invalid egg count"
        };

        var existingDailyRecord = DailyRecord.Create(tenantId, flockId, today, 10, "Original");

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockDailyRecordRepository.Setup(x => x.GetByIdWithoutNavigationAsync(dailyRecordId))
            .ReturnsAsync(existingDailyRecord);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Validation");
        result.Error.Message.Should().Contain("cannot be negative");

        _mockDailyRecordRepository.Verify(x => x.UpdateAsync(It.IsAny<DailyRecord>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNotesExceeding500Characters_ShouldReturnValidationError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var dailyRecordId = Guid.NewGuid();
        var flockId = Guid.NewGuid();
        var today = DateTime.UtcNow.Date;
        var longNotes = new string('A', 501);

        var command = new UpdateDailyRecordCommand
        {
            Id = dailyRecordId,
            EggCount = 20,
            Notes = longNotes
        };

        var existingDailyRecord = DailyRecord.Create(tenantId, flockId, today, 15, "Original");

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockDailyRecordRepository.Setup(x => x.GetByIdWithoutNavigationAsync(dailyRecordId))
            .ReturnsAsync(existingDailyRecord);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Validation");
        result.Error.Message.Should().Contain("cannot exceed 500 characters");

        _mockDailyRecordRepository.Verify(x => x.UpdateAsync(It.IsAny<DailyRecord>()), Times.Never);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldReturnFailureError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var dailyRecordId = Guid.NewGuid();
        var flockId = Guid.NewGuid();
        var today = DateTime.UtcNow.Date;

        var command = new UpdateDailyRecordCommand
        {
            Id = dailyRecordId,
            EggCount = 25
        };

        var existingDailyRecord = DailyRecord.Create(tenantId, flockId, today, 20, "Original");

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockDailyRecordRepository.Setup(x => x.GetByIdWithoutNavigationAsync(dailyRecordId))
            .ReturnsAsync(existingDailyRecord);
        _mockDailyRecordRepository.Setup(x => x.UpdateAsync(It.IsAny<DailyRecord>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Failure");
        result.Error.Message.Should().Contain("Failed to update daily record");
    }

    #endregion
}
